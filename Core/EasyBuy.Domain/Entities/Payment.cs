using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Payment : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Order Order { get; set; }
    public decimal Amount { get; set; }
    public required PaymentType PaymentType { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? PaymentGatewayResponse { get; set; }
}
