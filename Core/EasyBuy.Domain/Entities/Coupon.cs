using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Coupon : BaseEntity
{
    public required string Code { get; set; }
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; } = true;
    public CouponType Type { get; set; }
}

public enum CouponType
{
    Percentage,
    FixedAmount,
    FreeShipping
}
