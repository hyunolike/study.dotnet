using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.WebApi.Application.Coupons;
using StackExchange.Redis;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class CouponStockInitializer : IHostedService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IOptionsMonitor<CouponOptions> _optionsMonitor;
    private readonly ICouponCodeAllocator _couponCodeAllocator;
    private readonly ILogger<CouponStockInitializer> _logger;

    public CouponStockInitializer(
        IConnectionMultiplexer connectionMultiplexer,
        ICouponCodeAllocator couponCodeAllocator,
        IOptionsMonitor<CouponOptions> optionsMonitor,
        ILogger<CouponStockInitializer> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _couponCodeAllocator = couponCodeAllocator;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var definitions = _optionsMonitor.CurrentValue.Definitions;
        if (definitions.Count == 0)
        {
            _logger.LogInformation("No coupon definitions configured.");
            return;
        }

        var database = _connectionMultiplexer.GetDatabase();

        foreach (var definition in definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stockKey = $"coupon:{definition.Id}:stock";
            var issuedSetKey = $"coupon:{definition.Id}:issued";

            var availableCount = await _couponCodeAllocator.GetAvailableCountAsync(definition.Id, cancellationToken).ConfigureAwait(false);

            if (await database.StringSetAsync(stockKey, availableCount, when: When.NotExists).ConfigureAwait(false))
            {
                await database.KeyDeleteAsync(issuedSetKey).ConfigureAwait(false);
                _logger.LogInformation("Initialized coupon {CouponId} with available stock {Stock}.", definition.Id, availableCount);
                continue;
            }

            _logger.LogInformation("Coupon {CouponId} stock key already exists; skipping initialization.", definition.Id);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
