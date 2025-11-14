using EasyBuy.Application.Repositories.Payment;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories.Payments;

/// <summary>
/// Entity Framework implementation of IPaymentReadRepository.
/// </summary>
public class EfPaymentReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<Payment>(dbContext), IPaymentReadRepository
{
    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await dbContext.Set<Payment>()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
    {
        return await dbContext.Set<Payment>()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }
}
