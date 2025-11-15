using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderWriteRepository _orderWriteRepository;
    private readonly IOrderReadRepository _orderReadRepository;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderWriteRepository orderWriteRepository,
        IOrderReadRepository orderReadRepository,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderWriteRepository = orderWriteRepository;
        _orderReadRepository = orderReadRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderReadRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result.Failure($"Order {request.OrderId} not found");
            }

            // Parse status
            if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
            {
                return Result.Failure($"Invalid order status: {request.Status}");
            }

            // Update status based on new status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    if (string.IsNullOrEmpty(request.TrackingNumber))
                    {
                        return Result.Failure("Tracking number is required when shipping an order");
                    }
                    order.MarkAsShipped(request.TrackingNumber);
                    break;

                case OrderStatus.Delivered:
                    order.OrderStatus = OrderStatus.Delivered;
                    order.DeliveredDate = DateTime.UtcNow;
                    break;

                case OrderStatus.Processing:
                    if (order.OrderStatus != OrderStatus.Pending)
                    {
                        return Result.Failure("Only pending orders can be marked as processing");
                    }
                    order.OrderStatus = OrderStatus.Processing;
                    break;

                default:
                    order.OrderStatus = newStatus;
                    break;
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                order.Notes = string.IsNullOrEmpty(order.Notes)
                    ? request.Notes
                    : $"{order.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes}";
            }

            await _orderWriteRepository.UpdateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} status updated to {Status}",
                request.OrderId, newStatus);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating order {OrderId}", request.OrderId);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", request.OrderId);
            return Result.Failure($"Failed to update order status: {ex.Message}");
        }
    }
}
