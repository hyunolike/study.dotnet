using Sample.WebApi.Domain.Travel;

namespace Sample.WebApi.Application.Travel;

public interface ITravelProductQueryService
{
    Task<IReadOnlyList<TravelProduct>> GetAllAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);
}
