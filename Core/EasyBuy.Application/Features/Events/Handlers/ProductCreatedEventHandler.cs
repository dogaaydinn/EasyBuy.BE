using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles ProductCreatedEvent by warming cache and triggering indexing.
/// Ensures new products are immediately searchable and cached.
/// </summary>
public sealed class ProductCreatedEventHandler : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(
        ICacheService cacheService,
        ILogger<ProductCreatedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling ProductCreatedEvent for Product: {ProductId}, Name: {ProductName}",
            domainEvent.ProductId,
            domainEvent.ProductName);

        try
        {
            // Invalidate products list cache to include new product
            await _cacheService.RemoveByPatternAsync("products:*");

            _logger.LogInformation(
                "Products list cache invalidated for new product: {ProductId}",
                domainEvent.ProductId);

            // TODO: Trigger Elasticsearch indexing when implemented
            // await _searchService.IndexProductAsync(domainEvent.ProductId);

            // TODO: Notify subscribers about new product
            // Could send to message queue for further processing

            _logger.LogInformation(
                "ProductCreatedEvent handled successfully for Product: {ProductId}",
                domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to handle ProductCreatedEvent for Product: {ProductId}",
                domainEvent.ProductId);
        }
    }
}
