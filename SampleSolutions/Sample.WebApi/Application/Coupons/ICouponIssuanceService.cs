using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Application.Coupons;

public interface ICouponIssuanceService
{
    Task<CouponIssuanceResult> TryIssueAsync(
        string couponId,
        string userId,
        CancellationToken cancellationToken = default);
}
