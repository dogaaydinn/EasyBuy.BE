using EasyBuy.Application.Product;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Product;

public class EfProductWriteRepository(EasyBuyDbContext dbContext) : EfWriteRepository<Domain.Entities.Product>(dbContext), IProductWriteRepository
{
    
}