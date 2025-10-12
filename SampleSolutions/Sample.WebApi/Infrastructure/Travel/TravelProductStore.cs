using Sample.WebApi.Domain.Travel;

namespace Sample.WebApi.Infrastructure.Travel;

public sealed class TravelProductStore
{
    private const int DefaultProductCount = 5_000;

    private readonly SemaphoreSlim _updateLock = new(1, 1);
    private IReadOnlyList<TravelProduct> _products = Array.Empty<TravelProduct>();
    private long _version;

    public TravelProductStore()
    {
        Volatile.Write(ref _products, GenerateProducts(DefaultProductCount));
        Volatile.Write(ref _version, 1);
    }

    public event EventHandler? ProductsChanged;

    public long Version => Volatile.Read(ref _version);

    public async Task<IReadOnlyList<TravelProduct>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        return Volatile.Read(ref _products);
    }

    public async Task<int> RefreshAsync(int? requestedCount = null, CancellationToken cancellationToken = default)
    {
        int updatedCount;

        await _updateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);

            var count = requestedCount is > 0 ? requestedCount.Value : DefaultProductCount;
            var nextProducts = GenerateProducts(count);

            Volatile.Write(ref _products, nextProducts);
            Interlocked.Increment(ref _version);

            updatedCount = nextProducts.Count;
        }
        finally
        {
            _updateLock.Release();
        }

        OnProductsChanged();
        return updatedCount;
    }

    private static IReadOnlyList<TravelProduct> GenerateProducts(int count)
    {
        var random = new Random(Environment.TickCount);
        var destinations = new[]
        {
            "Seoul", "Busan", "Jeju", "Tokyo", "Osaka", "Bangkok", "Singapore", "Sydney", "Paris", "New York"
        };

        var generatedAt = DateTimeOffset.UtcNow;

        return Enumerable.Range(1, count)
            .Select(index =>
            {
                var destination = destinations[random.Next(destinations.Length)];
                return new TravelProduct(
                    Id: $"TP-{index:00000}",
                    Name: $"{destination} {generatedAt:yyyy-MM-dd HH:mm} 추천 상품 {index}",
                    Destination: destination,
                    Price: Math.Round(250_000m + (decimal)random.NextDouble() * 1_500_000m, 0),
                    Nights: random.Next(3, 12));
            })
            .ToArray();
    }

    private void OnProductsChanged()
    {
        ProductsChanged?.Invoke(this, EventArgs.Empty);
    }
}
