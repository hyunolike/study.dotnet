using Microsoft.Extensions.Caching.Memory;
using Sample.WebApi.Application.Travel;
using Sample.WebApi.Domain.Travel;

namespace Sample.WebApi.Infrastructure.Travel;

public sealed class CachedTravelProductQueryService : ITravelProductQueryService, IDisposable
{
    private const string CacheKey = "travel:products:all";

    private readonly IMemoryCache _cache;
    private readonly TravelProductStore _store;
    private readonly ILogger<CachedTravelProductQueryService> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    public CachedTravelProductQueryService(
        IMemoryCache cache,
        TravelProductStore store,
        ILogger<CachedTravelProductQueryService> logger)
    {
        _cache = cache;
        _store = store;
        _logger = logger;
        _store.ProductsChanged += HandleProductsChanged;
    }

    public async Task<IReadOnlyList<TravelProduct>> GetAllAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (!forceRefresh &&
            _cache.TryGetValue(CacheKey, out IReadOnlyList<TravelProduct>? cachedProducts) &&
            cachedProducts is not null)
        {
            _logger.LogDebug("Returning {Count} cached travel products.", cachedProducts.Count);
            return cachedProducts;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!forceRefresh &&
                _cache.TryGetValue(CacheKey, out cachedProducts) &&
                cachedProducts is not null)
            {
                _logger.LogDebug("Returning {Count} cached travel products after lock.", cachedProducts.Count);
                return cachedProducts;
            }

            var products = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
            _cache.Set(CacheKey, products, BuildDefaultCacheOptions());
            _logger.LogInformation("Cached {Count} travel products.", products.Count);
            return products;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _store.ProductsChanged -= HandleProductsChanged;
        _refreshLock.Dispose();
        _disposed = true;
    }

    private static MemoryCacheEntryOptions BuildDefaultCacheOptions()
    {
        return new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));
    }

    private void HandleProductsChanged(object? sender, EventArgs args)
    {
        _logger.LogInformation("Travel products changed. Clearing cached dataset.");
        _cache.Remove(CacheKey);
    }
}
