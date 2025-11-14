using EasyBuy.Application.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for OrderCreatedIntegrationEvent.
/// Processes order creation messages from the message queue.
/// Demonstrates enterprise message processing with error handling and logging.
/// </summary>
public sealed class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderCreatedIntegrationEvent: OrderId={OrderId}, UserId={UserId}, Total={TotalAmount}, MessageId={MessageId}",
            message.OrderId,
            message.UserId,
            message.TotalAmount,
            context.MessageId);

        try
        {
            // ================================================================
            // BUSINESS LOGIC PROCESSING
            // ================================================================

            // 1. Inventory reservation
            _logger.LogInformation(
                "Reserving inventory for {ItemCount} items in order {OrderId}",
                message.Items.Count,
                message.OrderId);

            foreach (var item in message.Items)
            {
                _logger.LogDebug(
                    "Reserving {Quantity}x {ProductName} (ProductId: {ProductId})",
                    item.Quantity,
                    item.ProductName,
                    item.ProductId);

                // TODO: Call inventory service to reserve stock
                // await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity);
            }

            // 2. Trigger analytics event
            _logger.LogInformation(
                "Recording order analytics for order {OrderId}",
                message.OrderId);

            // TODO: Publish to analytics service
            // await _analyticsService.TrackOrderAsync(message);

            // 3. Notify fraud detection
            _logger.LogInformation(
                "Sending order to fraud detection: {OrderId}, Amount: {Amount}",
                message.OrderId,
                message.TotalAmount);

            // TODO: If high-value order, trigger fraud check
            if (message.TotalAmount > 1000)
            {
                _logger.LogWarning(
                    "High-value order detected: {OrderId}, Amount: {Amount}. Triggering fraud check.",
                    message.OrderId,
                    message.TotalAmount);

                // await _fraudService.CheckOrderAsync(message.OrderId);
            }

            _logger.LogInformation(
                "Successfully processed OrderCreatedIntegrationEvent for order {OrderId}",
                message.OrderId);

            // Acknowledge message
            await context.Publish(new
            {
                OrderId = message.OrderId,
                ProcessedAt = DateTime.UtcNow,
                Status = "Processed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing OrderCreatedIntegrationEvent for order {OrderId}. Message will be retried.",
                message.OrderId);

            // Throw to trigger MassTransit retry policy
            throw;
        }
    }
}
