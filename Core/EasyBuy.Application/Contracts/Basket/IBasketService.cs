using EasyBuy.Application.DTOs.Baskets;

namespace EasyBuy.Application.Contracts.Basket;

/// <summary>
/// Redis-based basket service interface.
/// Provides shopping basket operations with 30-day expiration.
/// </summary>
public interface IBasketService
{
    /// <summary>
    /// Get basket for a user.
    /// </summary>
    Task<BasketDto?> GetBasketAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add item to basket or update quantity if already exists.
    /// </summary>
    Task<BasketDto> AddToBasketAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update basket item quantity.
    /// </summary>
    Task<BasketDto> UpdateBasketItemAsync(Guid userId, Guid basketItemId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove item from basket.
    /// </summary>
    Task<BasketDto> RemoveFromBasketAsync(Guid userId, Guid basketItemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear entire basket.
    /// </summary>
    Task ClearBasketAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete basket (used after order completion).
    /// </summary>
    Task DeleteBasketAsync(Guid userId, CancellationToken cancellationToken = default);
}
