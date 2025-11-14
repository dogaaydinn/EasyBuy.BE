namespace EasyBuy.Application.Repositories.Payment;

/// <summary>
/// Read repository for Payment entity.
/// Supports queries by order and transaction ID.
/// </summary>
public interface IPaymentReadRepository : IReadRepository<Domain.Entities.Payment>
{
    /// <summary>
    /// Get payment by order ID.
    /// </summary>
    Task<Domain.Entities.Payment?> GetByOrderIdAsync(Guid orderId);
    
    /// <summary>
    /// Get payment by transaction ID (Stripe payment intent ID).
    /// </summary>
    Task<Domain.Entities.Payment?> GetByTransactionIdAsync(string transactionId);
}
