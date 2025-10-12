namespace Sample.WebApi.Domain.Coupons;

public sealed record CouponDefinition(
    string Id,
    string? DisplayName,
    int TotalQuantity);
