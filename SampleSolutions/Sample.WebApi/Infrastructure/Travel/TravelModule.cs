using Microsoft.Extensions.DependencyInjection;
using Sample.WebApi.Application.Travel;

namespace Sample.WebApi.Infrastructure.Travel;

public static class TravelModule
{
    public static IServiceCollection AddTravelModule(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<TravelProductStore>();
        services.AddSingleton<ITravelProductQueryService, CachedTravelProductQueryService>();

        return services;
    }
}
