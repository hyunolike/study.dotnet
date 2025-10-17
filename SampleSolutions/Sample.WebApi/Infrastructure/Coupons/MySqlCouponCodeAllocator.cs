using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Sample.WebApi.Application.Coupons;
using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class MySqlCouponCodeAllocator : ICouponCodeAllocator
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlCouponCodeAllocator> _logger;

    public MySqlCouponCodeAllocator(IConfiguration configuration, ILogger<MySqlCouponCodeAllocator> logger)
    {
        _connectionString = configuration.GetConnectionString("MySql")
                             ?? throw new InvalidOperationException("MySql 연결 문자열이 설정되지 않았습니다.");
        _logger = logger;
    }

    public async Task<CouponCodeAllocationResult> AllocateAsync(string couponId, string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);

            var selectCommand = new MySqlCommand(
                """
                SELECT Id, CodeValue
                FROM CouponCodes
                WHERE CouponId = @couponId
                  AND Status = 0
                  AND AvailableFrom <= UTC_TIMESTAMP()
                ORDER BY AvailableFrom ASC, Id ASC
                LIMIT 1
                FOR UPDATE SKIP LOCKED;
                """,
                connection,
                transaction);

            selectCommand.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = couponId;

            using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                await reader.DisposeAsync().ConfigureAwait(false);
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                var upcomingCommand = new MySqlCommand(
                    """
                    SELECT COUNT(1)
                    FROM CouponCodes
                    WHERE CouponId = @couponId
                      AND Status = 0
                      AND AvailableFrom > UTC_TIMESTAMP();
                    """,
                    connection);

                upcomingCommand.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = couponId;
                var upcomingObj = await upcomingCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                var upcoming = Convert.ToInt64(upcomingObj ?? 0L);

                if (upcoming > 0)
                {
                    return new CouponCodeAllocationResult(
                        CouponCodeAllocationStatus.NotYetReleased,
                        null,
                        null,
                        "아직 오픈되지 않은 주차의 쿠폰입니다.");
                }

                return new CouponCodeAllocationResult(
                    CouponCodeAllocationStatus.NoCodesAvailable,
                    null,
                    null,
                    "발급 가능한 쿠폰 코드가 없습니다.");
            }

            if (reader.IsDBNull(0))
            {
                throw new InvalidOperationException("쿠폰 코드 식별자 값이 null입니다.");
            }

            var codeIdValue = reader.GetValue(0);
            var codeId = codeIdValue switch
            {
                Guid guid => guid.ToString(),
                byte[] bytes when bytes.Length == 16 => new Guid(bytes).ToString(),
                string value => value,
                _ => codeIdValue.ToString()
                     ?? throw new InvalidOperationException("쿠폰 코드 식별자를 문자열로 변환할 수 없습니다.")
            };
            var codeValue = reader.GetString(1);
            await reader.DisposeAsync().ConfigureAwait(false);

            var issuedAtUtc = DateTime.UtcNow;

            var updateCommand = new MySqlCommand(
                """
                UPDATE CouponCodes
                SET Status = 1,
                    UserId = @userId,
                    IssuedAt = @issuedAt
                WHERE Id = @id;
                """,
                connection,
                transaction);

            updateCommand.Parameters.Add("@id", MySqlDbType.VarChar, 36).Value = codeId;
            updateCommand.Parameters.Add("@userId", MySqlDbType.VarChar, 128).Value = userId;
            updateCommand.Parameters.Add("@issuedAt", MySqlDbType.DateTime).Value = issuedAtUtc;

            var updated = await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            if (updated != 1)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Coupon code update affected {RowCount} rows for coupon {CouponId}.", updated, couponId);
                return new CouponCodeAllocationResult(
                    CouponCodeAllocationStatus.UnknownFailure,
                    null,
                    null,
                    "쿠폰 코드 상태를 갱신하지 못했습니다.");
            }

            var historyCommand = new MySqlCommand(
                """
                INSERT INTO CouponIssueHistory (Id, CouponCodeId, CouponId, CodeValue, UserId, IssuedAt)
                VALUES (@id, @couponCodeId, @couponId, @codeValue, @userId, @issuedAt);
                """,
                connection,
                transaction);

            var historyId = Guid.NewGuid().ToString();
            historyCommand.Parameters.Add("@id", MySqlDbType.VarChar, 36).Value = historyId;
            historyCommand.Parameters.Add("@couponCodeId", MySqlDbType.VarChar, 36).Value = codeId;
            historyCommand.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = couponId;
            historyCommand.Parameters.Add("@codeValue", MySqlDbType.VarChar, 128).Value = codeValue;
            historyCommand.Parameters.Add("@userId", MySqlDbType.VarChar, 128).Value = userId;
            historyCommand.Parameters.Add("@issuedAt", MySqlDbType.DateTime).Value = issuedAtUtc;

            await historyCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new CouponCodeAllocationResult(
                CouponCodeAllocationStatus.Success,
                codeValue,
                new DateTimeOffset(issuedAtUtc, TimeSpan.Zero));
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "MySQL 오류로 쿠폰 코드를 할당하지 못했습니다.");
            return new CouponCodeAllocationResult(
                CouponCodeAllocationStatus.DatabaseUnavailable,
                null,
                null,
                "쿠폰 데이터베이스에 연결할 수 없습니다.");
        }
    }

    public async Task<int> GetAvailableCountAsync(string couponId, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new MySqlCommand(
                """
                SELECT COUNT(1)
                FROM CouponCodes
                WHERE CouponId = @couponId
                  AND Status = 0
                  AND AvailableFrom <= UTC_TIMESTAMP();
                """,
                connection);

            command.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = couponId;

            var resultObj = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return Convert.ToInt32(resultObj ?? 0L);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "MySQL 오류로 사용 가능한 쿠폰 수량을 조회하지 못했습니다.");
            return 0;
        }
    }
}
