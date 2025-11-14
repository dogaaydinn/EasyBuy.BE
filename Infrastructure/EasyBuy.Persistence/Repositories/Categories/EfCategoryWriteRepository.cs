using EasyBuy.Application.Repositories.Category;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Categories;

/// <summary>
/// Entity Framework implementation of ICategoryWriteRepository.
/// </summary>
public class EfCategoryWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<Category>(dbContext), ICategoryWriteRepository
{
}
