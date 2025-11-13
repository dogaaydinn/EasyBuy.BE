namespace EasyBuy.Domain.Events;

public class ProductInventoryChangedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }
    public string Reason { get; }

    public ProductInventoryChangedEvent(Guid productId, int oldQuantity, int newQuantity, string reason)
    {
        ProductId = productId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
        Reason = reason;
    }
}
