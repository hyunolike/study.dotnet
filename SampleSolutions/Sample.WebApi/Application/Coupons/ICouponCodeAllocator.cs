using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Application.Coupons;

public interface ICouponCodeAllocator
{
    Task<CouponCodeAllocationResult> AllocateAsync(
        string couponId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<int> GetAvailableCountAsync(
        string couponId,
        CancellationToken cancellationToken = default);
}
