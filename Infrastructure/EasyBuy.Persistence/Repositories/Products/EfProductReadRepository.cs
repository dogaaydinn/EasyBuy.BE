using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Products;

public class EfProductReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<Product>(dbContext), IProductReadRepository
{
}