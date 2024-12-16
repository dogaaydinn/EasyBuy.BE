using EasyBuy.Application.Repositories.Order;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Orders;

public class EfOrderWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<Order>(dbContext), IOrderWriteRepository
{
}