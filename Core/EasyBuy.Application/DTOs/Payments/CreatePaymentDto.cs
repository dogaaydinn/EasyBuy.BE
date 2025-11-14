namespace EasyBuy.Application.DTOs.Payments;

/// <summary>
/// Data transfer object for creating a payment.
/// </summary>
public sealed class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "card"; // "card", "bank_transfer", etc.
    public string? ReturnUrl { get; set; } // For redirect after payment
    public string? CancelUrl { get; set; } // For redirect on cancel
}
