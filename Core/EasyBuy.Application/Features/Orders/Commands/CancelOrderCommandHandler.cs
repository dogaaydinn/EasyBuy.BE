using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Handler for CancelOrderCommand.
/// Cancels order and restores product inventory.
/// </summary>
public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IOrderWriteRepository _orderRepository;
    private readonly IOrderReadRepository _orderReadRepository;
    private readonly IProductWriteRepository _productRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderWriteRepository orderRepository,
        IOrderReadRepository orderReadRepository,
        IProductWriteRepository productRepository,
        IDomainEventDispatcher eventDispatcher,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _orderReadRepository = orderReadRepository;
        _productRepository = productRepository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order: OrderId={OrderId}, Reason={Reason}",
            request.OrderId, request.CancellationReason);

        try
        {
            // Get order
            var order = await _orderReadRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result<bool>.Failure($"Order not found: {request.OrderId}");
            }

            // Check if order can be cancelled
            if (order.Status == "Shipped" || order.Status == "Delivered")
            {
                return Result<bool>.Failure($"Cannot cancel order in '{order.Status}' status. Please contact support for refund.");
            }

            if (order.Status == "Cancelled")
            {
                return Result<bool>.Success(true); // Idempotent
            }

            var oldStatus = order.Status;

            // Update order status
            order.Status = "Cancelled";
            order.Notes = $"Cancelled: {request.CancellationReason ?? "Customer request"}";

            await _orderRepository.UpdateAsync(order);

            // Restore inventory for each order item
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var oldStock = product.Stock;
                    product.Stock += item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    _logger.LogInformation(
                        "Restored inventory: ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}",
                        product.Id, oldStock, product.Stock);

                    // Dispatch inventory changed event
                    var inventoryEvent = new ProductInventoryChangedEvent(
                        product.Id,
                        product.Name,
                        oldStock,
                        product.Stock);

                    await _eventDispatcher.DispatchAsync(inventoryEvent, cancellationToken);
                }
            }

            _logger.LogInformation("Order cancelled successfully: OrderId={OrderId}", request.OrderId);

            // Dispatch order status changed event
            var statusChangedEvent = new OrderStatusChangedEvent(
                order.Id,
                order.UserId,
                "user@example.com", // TODO: Get from user
                "1234567890", // TODO: Get phone
                oldStatus,
                "Cancelled");

            await _eventDispatcher.DispatchAsync(statusChangedEvent, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order: OrderId={OrderId}", request.OrderId);
            return Result<bool>.Failure($"Failed to cancel order: {ex.Message}");
        }
    }
}
