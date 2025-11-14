using EasyBuy.Application.Contracts.Messaging;

namespace EasyBuy.Application.Messaging.IntegrationEvents;

/// <summary>
/// Integration event published when a payment is successfully processed.
/// Consumed by: Order Service, Accounting Service, Fraud Detection Service
/// </summary>
public sealed class PaymentProcessedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string PaymentMethod { get; init; } = string.Empty;
    public string TransactionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
