using EasyBuy.Application.Repositories.Payment;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Payments;

/// <summary>
/// Entity Framework implementation of IPaymentWriteRepository.
/// </summary>
public class EfPaymentWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<Payment>(dbContext), IPaymentWriteRepository
{
}
