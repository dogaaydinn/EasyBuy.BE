using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles ProductInventoryChangedEvent by invalidating product cache.
/// Ensures cache consistency when inventory changes.
/// </summary>
public sealed class ProductInventoryChangedEventHandler : IDomainEventHandler<ProductInventoryChangedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductInventoryChangedEventHandler> _logger;

    public ProductInventoryChangedEventHandler(
        ICacheService cacheService,
        ILogger<ProductInventoryChangedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(ProductInventoryChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling ProductInventoryChangedEvent for Product: {ProductId}, Stock changed from {OldStock} to {NewStock}",
            domainEvent.ProductId,
            domainEvent.OldStock,
            domainEvent.NewStock);

        try
        {
            // Invalidate product cache
            var productCacheKey = $"product:{domainEvent.ProductId}";
            await _cacheService.RemoveAsync(productCacheKey);

            _logger.LogInformation(
                "Cache invalidated for product: {ProductId}",
                domainEvent.ProductId);

            // Check for low stock warning
            if (domainEvent.NewStock < 10 && domainEvent.OldStock >= 10)
            {
                _logger.LogWarning(
                    "Low stock alert for Product: {ProductId}. Current stock: {Stock}",
                    domainEvent.ProductId,
                    domainEvent.NewStock);

                // TODO: Send notification to inventory management team
                // Could integrate with email or messaging system
            }

            // Check for out of stock
            if (domainEvent.NewStock == 0 && domainEvent.OldStock > 0)
            {
                _logger.LogWarning(
                    "Product out of stock: {ProductId}",
                    domainEvent.ProductId);

                // TODO: Notify product team and potentially trigger restocking workflow
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to handle ProductInventoryChangedEvent for Product: {ProductId}",
                domainEvent.ProductId);

            // Don't throw - cache invalidation failure shouldn't break inventory update
        }
    }
}
