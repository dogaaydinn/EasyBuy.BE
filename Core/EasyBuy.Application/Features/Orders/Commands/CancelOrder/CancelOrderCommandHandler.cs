using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderWriteRepository _orderWriteRepository;
    private readonly IOrderReadRepository _orderReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderWriteRepository orderWriteRepository,
        IOrderReadRepository orderReadRepository,
        IProductWriteRepository productWriteRepository,
        ICurrentUserService currentUserService,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderWriteRepository = orderWriteRepository;
        _orderReadRepository = orderReadRepository;
        _productWriteRepository = productWriteRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderReadRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result.Failure($"Order {request.OrderId} not found");
            }

            // Verify user owns the order (or is admin)
            var userId = _currentUserService.UserId;
            if (order.AppUserId != userId && !_currentUserService.IsInRole("Admin"))
            {
                return Result.Failure("You do not have permission to cancel this order");
            }

            // Cancel the order
            order.Cancel(request.Reason);

            // Restore inventory
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.Quantity += item.Quantity;
                }
            }

            await _orderWriteRepository.UpdateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} cancelled by user {UserId}. Reason: {Reason}",
                request.OrderId, userId, request.Reason);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel order {OrderId}", request.OrderId);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", request.OrderId);
            return Result.Failure($"Failed to cancel order: {ex.Message}");
        }
    }
}
