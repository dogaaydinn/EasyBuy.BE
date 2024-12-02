using EasyBuy.Application.Order;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Order;

public class EfOrderWriteRepository(EasyBuyDbContext dbContext) : EfWriteRepository<Domain.Entities.Order>(dbContext), IOrderWriteRepository
{
    
}