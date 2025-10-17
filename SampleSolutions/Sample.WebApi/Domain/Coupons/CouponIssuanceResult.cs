namespace Sample.WebApi.Domain.Coupons;

public enum CouponIssuanceStatus
{
    Success,
    CouponNotFound,
    AlreadyIssued,
    SoldOut,
    NotInitialized,
    InvalidRequest,
    InternalError,
}

public sealed record CouponIssuanceResult(
    CouponIssuanceStatus Status,
    string Message,
    int? RemainingQuantity = null,
    string? CouponCode = null,
    DateTimeOffset? IssuedAt = null)
{
    public bool Success => Status == CouponIssuanceStatus.Success;
}

public enum CouponCodeAllocationStatus
{
    Success,
    NoCodesAvailable,
    NotYetReleased,
    DatabaseUnavailable,
    UnknownFailure,
}

public sealed record CouponCodeAllocationResult(
    CouponCodeAllocationStatus Status,
    string? CouponCode,
    DateTimeOffset? IssuedAt,
    string? FailureReason = null)
{
    public bool Success => Status == CouponCodeAllocationStatus.Success;
}
