namespace EasyBuy.Application.DTOs.Payments;

/// <summary>
/// Data transfer object for Stripe Payment Intent response.
/// Contains client secret for frontend to complete payment.
/// </summary>
public sealed class PaymentIntentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
}
