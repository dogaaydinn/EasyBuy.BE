namespace EasyBuy.Application.Repositories.Review;

/// <summary>
/// Read repository for Review entity.
/// Supports queries by product and user.
/// </summary>
public interface IReviewReadRepository : IReadRepository<Domain.Entities.Review>
{
    /// <summary>
    /// Get all reviews for a specific product.
    /// </summary>
    Task<IEnumerable<Domain.Entities.Review>> GetProductReviewsAsync(Guid productId);
    
    /// <summary>
    /// Get all reviews by a specific user.
    /// </summary>
    Task<IEnumerable<Domain.Entities.Review>> GetUserReviewsAsync(Guid userId);
    
    /// <summary>
    /// Get average rating for a product.
    /// </summary>
    Task<double> GetProductAverageRatingAsync(Guid productId);
    
    /// <summary>
    /// Check if user has already reviewed a product.
    /// </summary>
    Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId);
}
