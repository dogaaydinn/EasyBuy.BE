using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Enums;

public class PaymentType : BaseEntity<int>
{
    public string Name { get; set; } // Payment type name (e.g., Credit Card, PayPal)
    public bool IsActive { get; set; } // Whether the payment type is active
}