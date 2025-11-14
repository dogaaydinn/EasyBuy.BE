using EasyBuy.Application.Repositories.Review;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories.Reviews;

/// <summary>
/// Entity Framework implementation of IReviewReadRepository.
/// Provides review queries with user and product eager loading.
/// </summary>
public class EfReviewReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<Review>(dbContext), IReviewReadRepository
{
    public async Task<IEnumerable<Review>> GetProductReviewsAsync(Guid productId)
    {
        return await dbContext.Set<Review>()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetUserReviewsAsync(Guid userId)
    {
        return await dbContext.Set<Review>()
            .Where(r => r.UserId == userId)
            .Include(r => r.Product)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<double> GetProductAverageRatingAsync(Guid productId)
    {
        var reviews = await dbContext.Set<Review>()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync();

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId)
    {
        return await dbContext.Set<Review>()
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }
}
