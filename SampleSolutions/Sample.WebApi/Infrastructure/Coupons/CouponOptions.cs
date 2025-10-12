using System.ComponentModel.DataAnnotations;
using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class CouponOptions
{
    public IList<CouponDefinitionOptions> Definitions { get; init; } = new List<CouponDefinitionOptions>();

    public CouponDefinition? Find(string couponId)
    {
        var option = Definitions.FirstOrDefault(def => string.Equals(def.Id, couponId, StringComparison.OrdinalIgnoreCase));
        return option is null
            ? null
            : new CouponDefinition(option.Id, option.DisplayName, option.TotalQuantity);
    }
}

public sealed class CouponDefinitionOptions
{
    [Required]
    public required string Id { get; init; }

    public string? DisplayName { get; init; }

    [Range(1, int.MaxValue)]
    public int TotalQuantity { get; init; }
}
