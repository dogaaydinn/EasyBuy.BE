using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for InventoryUpdatedIntegrationEvent.
/// Handles inventory synchronization across services and cache invalidation.
/// </summary>
public sealed class InventoryUpdatedConsumer : IConsumer<InventoryUpdatedIntegrationEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<InventoryUpdatedConsumer> _logger;

    public InventoryUpdatedConsumer(
        ICacheService cacheService,
        ILogger<InventoryUpdatedConsumer> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing InventoryUpdatedIntegrationEvent: ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}, Reason={Reason}",
            message.ProductId,
            message.OldStock,
            message.NewStock,
            message.Reason);

        try
        {
            // 1. Invalidate product cache
            var productCacheKey = $"product:{message.ProductId}";
            await _cacheService.RemoveAsync(productCacheKey);

            _logger.LogInformation(
                "Invalidated cache for product {ProductId}",
                message.ProductId);

            // 2. Update search index
            _logger.LogDebug(
                "Updating search index for product {ProductId}",
                message.ProductId);

            // TODO: Update Elasticsearch index
            // await _searchService.UpdateProductStockAsync(message.ProductId, message.NewStock);

            // 3. Check for low stock alerts
            if (message.IsLowStock)
            {
                _logger.LogWarning(
                    "Low stock alert for product {ProductId} ({ProductName}): {NewStock} units remaining",
                    message.ProductId,
                    message.ProductName,
                    message.NewStock);

                // TODO: Publish low stock alert
                // await context.Publish(new LowStockAlertEvent
                // {
                //     ProductId = message.ProductId,
                //     ProductName = message.ProductName,
                //     CurrentStock = message.NewStock
                // });
            }

            // 4. If out of stock, notify relevant services
            if (message.NewStock == 0)
            {
                _logger.LogWarning(
                    "Product out of stock: {ProductId} ({ProductName})",
                    message.ProductId,
                    message.ProductName);

                // TODO: Trigger automatic restock workflow
                // TODO: Notify marketing team (stop ads for this product)
                // await _restockService.TriggerRestockAsync(message.ProductId);
            }

            _logger.LogInformation(
                "Successfully processed InventoryUpdatedIntegrationEvent for product {ProductId}",
                message.ProductId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing InventoryUpdatedIntegrationEvent for product {ProductId}. Message will be retried.",
                message.ProductId);

            throw;
        }
    }
}
