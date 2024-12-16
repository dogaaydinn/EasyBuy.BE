using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Products;

public class EfProductWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<Product>(dbContext), IProductWriteRepository
{
}