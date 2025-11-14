using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.DTOs.Baskets;
using EasyBuy.Application.Repositories.Product;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EasyBuy.Infrastructure.Services.Basket;

/// <summary>
/// Redis-based basket service implementation.
/// Stores baskets in Redis with 30-day expiration and automatic cleanup.
/// </summary>
public class RedisBasketService : IBasketService
{
    private readonly IDistributedCache _cache;
    private readonly IProductReadRepository _productRepository;
    private readonly ILogger<RedisBasketService> _logger;
    private static readonly TimeSpan BasketExpiration = TimeSpan.FromDays(30);

    public RedisBasketService(
        IDistributedCache cache,
        IProductReadRepository productRepository,
        ILogger<RedisBasketService> logger)
    {
        _cache = cache;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<BasketDto?> GetBasketAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = GetBasketKey(userId);
        var basketJson = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(basketJson))
        {
            _logger.LogInformation("No basket found for user: {UserId}", userId);
            return null;
        }

        var basket = JsonSerializer.Deserialize<BasketDto>(basketJson);
        _logger.LogInformation("Retrieved basket for user: {UserId}, Items: {ItemCount}", userId, basket?.ItemCount ?? 0);
        
        return basket;
    }

    public async Task<BasketDto> AddToBasketAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding to basket: User={UserId}, Product={ProductId}, Quantity={Quantity}", 
            userId, productId, quantity);

        // Get product details
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product not found: {productId}");
        }

        // Check stock availability
        if (product.Quantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {product.Quantity}, Requested: {quantity}");
        }

        // Get or create basket
        var basket = await GetBasketAsync(userId, cancellationToken) ?? new BasketDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items = new List<BasketItemDto>()
        };

        // Check if product already in basket
        var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            // Update quantity
            var newQuantity = existingItem.Quantity + quantity;
            if (product.Quantity < newQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Quantity}, Total requested: {newQuantity}");
            }
            existingItem.Quantity = newQuantity;
            _logger.LogInformation("Updated existing basket item quantity: {NewQuantity}", newQuantity);
        }
        else
        {
            // Add new item
            var basketItem = new BasketItemDto
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ProductImageFiles.FirstOrDefault()?.Path,
                IsInStock = product.Quantity > 0
            };
            basket.Items.Add(basketItem);
            _logger.LogInformation("Added new item to basket: {ProductName}", product.Name);
        }

        await SaveBasketAsync(basket, cancellationToken);
        return basket;
    }

    public async Task<BasketDto> UpdateBasketItemAsync(Guid userId, Guid basketItemId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating basket item: User={UserId}, ItemId={BasketItemId}, NewQuantity={Quantity}", 
            userId, basketItemId, quantity);

        var basket = await GetBasketAsync(userId, cancellationToken);
        if (basket == null)
        {
            throw new InvalidOperationException("Basket not found");
        }

        var item = basket.Items.FirstOrDefault(i => i.Id == basketItemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Basket item not found: {basketItemId}");
        }

        // Check stock availability
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product not found: {item.ProductId}");
        }

        if (product.Quantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {product.Quantity}, Requested: {quantity}");
        }

        item.Quantity = quantity;
        item.IsInStock = product.Quantity > 0;

        await SaveBasketAsync(basket, cancellationToken);
        _logger.LogInformation("Updated basket item quantity to: {NewQuantity}", quantity);
        
        return basket;
    }

    public async Task<BasketDto> RemoveFromBasketAsync(Guid userId, Guid basketItemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing from basket: User={UserId}, ItemId={BasketItemId}", userId, basketItemId);

        var basket = await GetBasketAsync(userId, cancellationToken);
        if (basket == null)
        {
            throw new InvalidOperationException("Basket not found");
        }

        var item = basket.Items.FirstOrDefault(i => i.Id == basketItemId);
        if (item != null)
        {
            basket.Items.Remove(item);
            _logger.LogInformation("Removed item from basket: {ProductName}", item.ProductName);
            
            await SaveBasketAsync(basket, cancellationToken);
        }

        return basket;
    }

    public async Task ClearBasketAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing basket for user: {UserId}", userId);

        var basket = await GetBasketAsync(userId, cancellationToken);
        if (basket != null)
        {
            basket.Items.Clear();
            await SaveBasketAsync(basket, cancellationToken);
        }
    }

    public async Task DeleteBasketAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting basket for user: {UserId}", userId);

        var key = GetBasketKey(userId);
        await _cache.RemoveAsync(key, cancellationToken);
    }

    private async Task SaveBasketAsync(BasketDto basket, CancellationToken cancellationToken)
    {
        var key = GetBasketKey(basket.UserId);
        var basketJson = JsonSerializer.Serialize(basket);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = BasketExpiration
        };

        await _cache.SetStringAsync(key, basketJson, options, cancellationToken);
        _logger.LogInformation("Saved basket to Redis with 30-day expiration: User={UserId}, Items={ItemCount}, Total={Total}", 
            basket.UserId, basket.ItemCount, basket.Total);
    }

    private static string GetBasketKey(Guid userId) => $"basket:{userId}";
}
