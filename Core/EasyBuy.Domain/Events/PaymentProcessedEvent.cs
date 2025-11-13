namespace EasyBuy.Domain.Events;

public class PaymentProcessedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string PaymentMethod { get; }
    public bool IsSuccessful { get; }

    public PaymentProcessedEvent(Guid orderId, Guid paymentId, decimal amount, string paymentMethod, bool isSuccessful)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        IsSuccessful = isSuccessful;
    }
}
