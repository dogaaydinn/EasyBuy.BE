using EasyBuy.Application.Repositories.File;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Files;

public class EfProductImageFileReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<ProductImageFile>(dbContext), IProductImageFileReadRepository
{
}