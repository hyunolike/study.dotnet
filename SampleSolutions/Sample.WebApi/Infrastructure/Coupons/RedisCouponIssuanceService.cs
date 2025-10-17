using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.WebApi.Application.Coupons;
using Sample.WebApi.Domain.Coupons;
using StackExchange.Redis;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class RedisCouponIssuanceService : ICouponIssuanceService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ICouponCodeAllocator _couponCodeAllocator;
    private readonly IOptionsMonitor<CouponOptions> _optionsMonitor;
    private readonly ILogger<RedisCouponIssuanceService> _logger;

    private static readonly LuaScript IssueScript = LuaScript.Prepare(
        """
        local stockKey = @stockKey
        local issuedSetKey = @issuedSetKey
        local userId = @userId

        if not userId or string.len(userId) == 0 then
            return -4
        end

        if redis.call('SISMEMBER', issuedSetKey, userId) == 1 then
            return -1
        end

        local stock = redis.call('GET', stockKey)
        if not stock then
            return -3
        end

        stock = tonumber(stock)
        if stock <= 0 then
            return -2
        end

        redis.call('DECR', stockKey)
        redis.call('SADD', issuedSetKey, userId)
        return stock - 1
        """);

    public RedisCouponIssuanceService(
        IConnectionMultiplexer connectionMultiplexer,
        ICouponCodeAllocator couponCodeAllocator,
        IOptionsMonitor<CouponOptions> optionsMonitor,
        ILogger<RedisCouponIssuanceService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _couponCodeAllocator = couponCodeAllocator;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task<CouponIssuanceResult> TryIssueAsync(string couponId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(couponId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var definition = _optionsMonitor.CurrentValue.Find(couponId);
        if (definition is null)
        {
            _logger.LogWarning("Coupon {CouponId} is not configured.", couponId);
            return new CouponIssuanceResult(
                CouponIssuanceStatus.CouponNotFound,
                $"쿠폰 {couponId} 정보를 찾을 수 없습니다.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();
        var stockKey = GetStockKey(couponId);
        var issuedSetKey = GetIssuedSetKey(couponId);

        try
        {
            var rawResult = await IssueScript.EvaluateAsync(
                database,
                new
                {
                    stockKey,
                    issuedSetKey,
                    userId = (RedisValue)userId
                }).ConfigureAwait(false);
            if (rawResult.IsNull)
            {
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.NotInitialized,
                    $"쿠폰 {couponId} 재고가 초기화되지 않았습니다.");
            }

            var resultCode = (long)rawResult;
            return await HandleResultAsync(
                couponId,
                userId,
                resultCode,
                stockKey,
                issuedSetKey,
                cancellationToken).ConfigureAwait(false);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis 연결 중 오류가 발생했습니다.");
            return new CouponIssuanceResult(
                CouponIssuanceStatus.NotInitialized,
                "쿠폰 발급 시스템이 준비되지 않았습니다. 잠시 후 다시 시도해 주세요.");
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis 처리 중 오류가 발생했습니다.");
            return new CouponIssuanceResult(
                CouponIssuanceStatus.InvalidRequest,
                "쿠폰 발급 처리 중 문제가 발생했습니다.");
        }
    }

    private async Task<CouponIssuanceResult> HandleResultAsync(
        string couponId,
        string userId,
        long scriptResult,
        RedisKey stockKey,
        RedisKey issuedSetKey,
        CancellationToken cancellationToken)
    {
        switch (scriptResult)
        {
            case >= 0:
                return await HandleSuccessfulReservationAsync(
                    couponId,
                    userId,
                    (int)scriptResult,
                    stockKey,
                    issuedSetKey,
                    cancellationToken).ConfigureAwait(false);
            case -1:
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.AlreadyIssued,
                    "이미 해당 쿠폰을 발급받았습니다.");
            case -2:
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.SoldOut,
                    "쿠폰 재고가 모두 소진되었습니다.");
            case -3:
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.NotInitialized,
                    $"쿠폰 {couponId} 재고가 초기화되지 않았습니다.");
            case -4:
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.InvalidRequest,
                    "유효하지 않은 사용자 정보입니다.");
            default:
                return new CouponIssuanceResult(
                    CouponIssuanceStatus.InvalidRequest,
                    "쿠폰 발급 처리 중 알 수 없는 오류가 발생했습니다.");
        }
    }

    private async Task<CouponIssuanceResult> HandleSuccessfulReservationAsync(
        string couponId,
        string userId,
        int remaining,
        RedisKey stockKey,
        RedisKey issuedSetKey,
        CancellationToken cancellationToken)
    {
        var allocation = await _couponCodeAllocator.AllocateAsync(couponId, userId, cancellationToken).ConfigureAwait(false);
        if (allocation.Success)
        {
            return new CouponIssuanceResult(
                CouponIssuanceStatus.Success,
                $"쿠폰 {couponId} 발급이 완료되었습니다.",
                remaining,
                allocation.CouponCode,
                allocation.IssuedAt);
        }

        await RestoreReservationAsync(stockKey, issuedSetKey, userId).ConfigureAwait(false);

        return allocation.Status switch
        {
            CouponCodeAllocationStatus.NoCodesAvailable => new CouponIssuanceResult(
                CouponIssuanceStatus.SoldOut,
                allocation.FailureReason ?? "발급 가능한 쿠폰 코드가 모두 소진되었습니다.",
                remaining + 1),
            CouponCodeAllocationStatus.NotYetReleased => new CouponIssuanceResult(
                CouponIssuanceStatus.NotInitialized,
                allocation.FailureReason ?? "현재는 쿠폰 코드가 열려 있지 않습니다.",
                remaining + 1),
            CouponCodeAllocationStatus.DatabaseUnavailable => new CouponIssuanceResult(
                CouponIssuanceStatus.InternalError,
                allocation.FailureReason ?? "쿠폰 데이터베이스에 연결할 수 없습니다.",
                remaining + 1),
            _ => new CouponIssuanceResult(
                CouponIssuanceStatus.InternalError,
                allocation.FailureReason ?? "쿠폰 코드 발급 처리 중 문제가 발생했습니다.",
                remaining + 1),
        };
    }

    private async Task RestoreReservationAsync(RedisKey stockKey, RedisKey issuedSetKey, string userId)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.StringIncrementAsync(stockKey).ConfigureAwait(false);
            await database.SetRemoveAsync(issuedSetKey, userId).ConfigureAwait(false);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis 재고 롤백 중 오류가 발생했습니다. stockKey={StockKey}, userId={UserId}", stockKey, userId);
        }
    }

    private static RedisKey GetStockKey(string couponId) => $"coupon:{couponId}:stock";

    private static RedisKey GetIssuedSetKey(string couponId) => $"coupon:{couponId}:issued";
}
