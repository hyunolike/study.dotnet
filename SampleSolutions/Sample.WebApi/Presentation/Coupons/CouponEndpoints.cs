using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sample.WebApi.Application.Coupons;
using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Presentation.Coupons;

public static class CouponEndpoints
{
    public static IEndpointRouteBuilder MapCouponEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/coupons")
            .WithTags("Coupons");

        group.MapPost("/issue", async Task<IResult> (
                [FromServices] ICouponIssuanceService issuanceService,
                [FromBody] CouponIssueRequest request,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.CouponId))
                {
                    return Results.BadRequest(new CouponIssueResponse(
                        false,
                        "CouponId 값이 필요합니다.",
                        null));
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return Results.BadRequest(new CouponIssueResponse(
                        false,
                        "UserId 값이 필요합니다.",
                        null));
                }

                var result = await issuanceService.TryIssueAsync(request.CouponId, request.UserId, cancellationToken);

                return result.Status switch
                {
                    CouponIssuanceStatus.Success => Results.Ok(new CouponIssueResponse(true, result.Message, result.RemainingQuantity)),
                    CouponIssuanceStatus.CouponNotFound => Results.NotFound(new CouponIssueResponse(false, result.Message, null)),
                    CouponIssuanceStatus.SoldOut => Results.Conflict(new CouponIssueResponse(false, result.Message, 0)),
                    CouponIssuanceStatus.AlreadyIssued => Results.Conflict(new CouponIssueResponse(false, result.Message, result.RemainingQuantity)),
                    CouponIssuanceStatus.NotInitialized => Results.Json(
                        new CouponIssueResponse(false, result.Message, result.RemainingQuantity),
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                    _ => Results.BadRequest(new CouponIssueResponse(false, result.Message, result.RemainingQuantity))
                };
            })
            .WithName("IssueCoupon")
            .WithDescription("선착순 쿠폰을 발급합니다. 재고가 소진되거나 이미 발급받은 경우 실패합니다.");

        return endpoints;
    }

    private sealed record CouponIssueRequest(string CouponId, string UserId);

    private sealed record CouponIssueResponse(bool Success, string Message, int? RemainingQuantity);
}
