using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Domain.Enums;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Manager");

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure("Order not found");
            }

            // Check authorization - user can only cancel their own orders (unless admin)
            if (!isAdmin && order.UserId != userId)
            {
                return Result<bool>.Failure("You are not authorized to cancel this order");
            }

            // Check if order can be cancelled
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Completed)
            {
                return Result<bool>.Failure("Cannot cancel a delivered or completed order");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Result<bool>.Failure("Order is already cancelled");
            }

            // Use UpdateOrderStatus command to handle the cancellation
            var updateStatusCommand = new UpdateOrderStatus.UpdateOrderStatusCommand
            {
                OrderId = request.OrderId,
                NewStatus = OrderStatus.Cancelled,
                Reason = request.Reason
            };

            var result = await _mediator.Send(updateStatusCommand, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Order {OrderNumber} cancelled by user {UserId}",
                    order.OrderNumber, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", request.OrderId);
            return Result<bool>.Failure($"An error occurred while cancelling the order: {ex.Message}");
        }
    }
}
