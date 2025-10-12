namespace Sample.WebApi.Domain.Coupons;

public enum CouponIssuanceStatus
{
    Success,
    CouponNotFound,
    AlreadyIssued,
    SoldOut,
    NotInitialized,
    InvalidRequest,
}

public sealed record CouponIssuanceResult(
    CouponIssuanceStatus Status,
    string Message,
    int? RemainingQuantity = null)
{
    public bool Success => Status == CouponIssuanceStatus.Success;
}
