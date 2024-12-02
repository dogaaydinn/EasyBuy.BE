using EasyBuy.Application.Order;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Order;

public class EfOrderReadRepository(EasyBuyDbContext dbContext) : EfReadRepository<Domain.Entities.Order>(dbContext), IOrderReadRepository
{
    
}