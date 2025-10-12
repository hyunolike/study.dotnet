using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class CouponStockInitializer : IHostedService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IOptionsMonitor<CouponOptions> _optionsMonitor;
    private readonly ILogger<CouponStockInitializer> _logger;

    public CouponStockInitializer(
        IConnectionMultiplexer connectionMultiplexer,
        IOptionsMonitor<CouponOptions> optionsMonitor,
        ILogger<CouponStockInitializer> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
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

            if (await database.StringSetAsync(
                    stockKey,
                    definition.TotalQuantity,
                    when: When.NotExists).ConfigureAwait(false))
            {
                await database.KeyDeleteAsync(issuedSetKey).ConfigureAwait(false);
                _logger.LogInformation("Initialized coupon {CouponId} with stock {Stock}.", definition.Id, definition.TotalQuantity);
                continue;
            }

            _logger.LogInformation("Coupon {CouponId} stock key already exists; skipping initialization.", definition.Id);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
