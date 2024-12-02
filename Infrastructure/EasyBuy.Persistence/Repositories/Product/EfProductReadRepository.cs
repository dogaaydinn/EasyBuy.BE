using EasyBuy.Application.Product;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Product;

public class EfProductReadRepository(EasyBuyDbContext dbContext) : EfReadRepository<Domain.Entities.Product>(dbContext), IProductReadRepository
{
    
}