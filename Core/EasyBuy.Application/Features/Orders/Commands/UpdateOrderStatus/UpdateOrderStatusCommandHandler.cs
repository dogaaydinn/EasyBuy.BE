using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Events;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IPublisher publisher,
        IEmailService emailService,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _publisher = publisher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure("Order not found");
            }

            // Store old status for event
            var oldStatus = order.Status;

            // Validate status transition
            if (!IsValidStatusTransition(oldStatus, request.NewStatus))
            {
                return Result<bool>.Failure($"Invalid status transition from {oldStatus} to {request.NewStatus}");
            }

            // Update order status
            order.Status = request.NewStatus;
            order.ModifiedDate = DateTime.UtcNow;

            // Handle specific status updates
            switch (request.NewStatus)
            {
                case OrderStatus.Shipped:
                    if (!string.IsNullOrEmpty(request.TrackingNumber))
                    {
                        // In real implementation, update delivery tracking
                        _logger.LogInformation("Order {OrderNumber} shipped with tracking number: {TrackingNumber}",
                            order.OrderNumber, request.TrackingNumber);
                    }
                    break;

                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;

                case OrderStatus.Cancelled:
                    // Restore product stock
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == order.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var item in orderItems)
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            product.ModifiedDate = DateTime.UtcNow;

                            await _publisher.Publish(new ProductInventoryChangedEvent(
                                product.Id,
                                product.Name,
                                product.Stock,
                                item.Quantity), cancellationToken);
                        }
                    }

                    // Update payment status if exists
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.OrderId == order.Id, cancellationToken);

                    if (payment != null && payment.Status == PaymentStatus.Completed)
                    {
                        payment.Status = PaymentStatus.Refunded;
                        payment.ModifiedDate = DateTime.UtcNow;
                    }

                    _logger.LogInformation("Order {OrderNumber} cancelled. Reason: {Reason}",
                        order.OrderNumber, request.Reason ?? "Not specified");
                    break;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderNumber} status updated from {OldStatus} to {NewStatus}",
                order.OrderNumber, oldStatus, request.NewStatus);

            // Publish order status changed event
            await _publisher.Publish(new OrderStatusChangedEvent(
                order.Id,
                order.OrderNumber,
                oldStatus,
                request.NewStatus,
                request.Reason), cancellationToken);

            // Send notification email to customer
            if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
            {
                var subject = $"Order {order.OrderNumber} Status Update";
                var body = $"Your order status has been updated to: {request.NewStatus}";

                if (!string.IsNullOrEmpty(request.TrackingNumber))
                {
                    body += $"\nTracking Number: {request.TrackingNumber}";
                }

                _ = _emailService.SendEmailAsync(order.User.Email, subject, body, cancellationToken);
            }

            return Result<bool>.Success(true, $"Order status updated to {request.NewStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", request.OrderId);
            return Result<bool>.Failure($"An error occurred while updating order status: {ex.Message}");
        }
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid status transitions
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Processing) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Delivered, OrderStatus.Completed) => true,
            _ => false
        };
    }
}
