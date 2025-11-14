using EasyBuy.Application.Repositories.Review;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Reviews;

/// <summary>
/// Entity Framework implementation of IReviewWriteRepository.
/// </summary>
public class EfReviewWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<Review>(dbContext), IReviewWriteRepository
{
}
