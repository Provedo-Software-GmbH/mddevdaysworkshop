using System.ComponentModel;

namespace DevConfTicketing.Domain.Vouchers;

[Description("Defines how a voucher discount is calculated")]
public enum DiscountType
{
    [Description("Discount is a percentage of the original price")]
    Percentage,

    [Description("Discount is a fixed absolute amount subtracted from the price")]
    Absolute,

    [Description("Discount sets the price to a fixed value")]
    FixedPrice
}
