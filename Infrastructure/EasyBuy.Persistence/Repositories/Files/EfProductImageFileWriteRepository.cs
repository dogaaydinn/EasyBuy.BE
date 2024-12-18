using EasyBuy.Application.Repositories.File;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Files;

public class EfProductImageFileWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<ProductImageFile>(dbContext), IProductImageFileWriteRepository
{
}