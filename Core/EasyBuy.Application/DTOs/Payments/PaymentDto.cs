namespace EasyBuy.Application.DTOs.Payments;

/// <summary>
/// Data transfer object for Payment entity.
/// </summary>
public sealed class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
