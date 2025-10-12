using Microsoft.AspNetCore.Mvc;
using Sample.WebApi.Application.Travel;
using Sample.WebApi.Infrastructure.Travel;

namespace Sample.WebApi.Presentation.Travel;

public static class TravelEndpoints
{
    public static IEndpointRouteBuilder MapTravelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/travel-products")
            .WithTags("Travel");

        group.MapGet("/", async Task<IResult> (
                [FromServices] ITravelProductQueryService queryService,
                [FromServices] TravelProductStore store,
                [FromQuery] bool forceRefresh,
                CancellationToken cancellationToken) =>
            {
                var products = await queryService.GetAllAsync(forceRefresh, cancellationToken);
                return Results.Ok(new TravelProductResponse(products.Count, store.Version, products));
            })
            .WithName("GetTravelProducts")
            .WithDescription("대량의 여행 상품 정보를 인메모리 캐시로 조회합니다.");

        group.MapPost("/refresh", async Task<IResult> (
                [FromServices] TravelProductStore store,
                [FromQuery] int? desiredCount,
                CancellationToken cancellationToken) =>
            {
                var updated = await store.RefreshAsync(desiredCount, cancellationToken);
                return Results.Ok(new TravelProductRefreshResponse(
                    Message: "여행 상품 데이터가 갱신되었습니다.",
                    UpdatedCount: updated,
                    Version: store.Version));
            })
            .WithName("RefreshTravelProducts")
            .WithDescription("데이터 소스 변경을 가정하고 여행 상품 데이터를 갱신한 뒤 캐시를 무효화합니다.");

        return endpoints;
    }

    private sealed record TravelProductResponse(int Count, long Version, IReadOnlyList<Domain.Travel.TravelProduct> Items);

    private sealed record TravelProductRefreshResponse(string Message, int UpdatedCount, long Version);
}
