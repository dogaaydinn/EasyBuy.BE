using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Handler for UpdateOrderStatusCommand.
/// Updates order status and dispatches OrderStatusChangedEvent for notifications.
/// </summary>
public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IOrderWriteRepository _orderRepository;
    private readonly IOrderReadRepository _orderReadRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    private static readonly string[] ValidStatuses = { "Created", "Processing", "Shipped", "Delivered", "Cancelled" };

    public UpdateOrderStatusCommandHandler(
        IOrderWriteRepository orderRepository,
        IOrderReadRepository orderReadRepository,
        IDomainEventDispatcher eventDispatcher,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _orderReadRepository = orderReadRepository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating order status: OrderId={OrderId}, NewStatus={Status}", request.OrderId, request.Status);

        try
        {
            // Validate status
            if (!ValidStatuses.Contains(request.Status))
            {
                return Result<bool>.Failure($"Invalid status: {request.Status}. Valid statuses: {string.Join(", ", ValidStatuses)}");
            }

            // Get order
            var order = await _orderReadRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result<bool>.Failure($"Order not found: {request.OrderId}");
            }

            var oldStatus = order.Status;

            // Validate status transition
            if (!IsValidStatusTransition(oldStatus, request.Status))
            {
                return Result<bool>.Failure($"Invalid status transition from {oldStatus} to {request.Status}");
            }

            // Update order status
            order.Status = request.Status;

            if (!string.IsNullOrEmpty(request.TrackingNumber))
            {
                order.TrackingNumber = request.TrackingNumber;
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                order.Notes = request.Notes;
            }

            // Update timestamps based on status
            switch (request.Status)
            {
                case "Shipped":
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case "Delivered":
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
            }

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Order status updated: OrderId={OrderId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
                request.OrderId, oldStatus, request.Status);

            // Dispatch domain event
            var statusChangedEvent = new OrderStatusChangedEvent(
                order.Id,
                order.UserId,
                "user@example.com", // TODO: Get from user
                "1234567890", // TODO: Get phone from user
                oldStatus,
                request.Status);

            await _eventDispatcher.DispatchAsync(statusChangedEvent, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status: OrderId={OrderId}", request.OrderId);
            return Result<bool>.Failure($"Failed to update order status: {ex.Message}");
        }
    }

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        // Define valid status transitions
        return (currentStatus, newStatus) switch
        {
            ("Created", "Processing") => true,
            ("Created", "Cancelled") => true,
            ("Processing", "Shipped") => true,
            ("Processing", "Cancelled") => true,
            ("Shipped", "Delivered") => true,
            ("Shipped", "Cancelled") => false, // Can't cancel shipped orders
            _ => currentStatus == newStatus // Allow same status (idempotent)
        };
    }
}
