using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Orders.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderReadRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderReadRepository orderRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result<OrderDto>.Failure($"Order {request.OrderId} not found");
            }

            // Verify user owns the order or is admin
            var userId = _currentUserService.UserId;
            if (order.AppUserId != userId && !_currentUserService.IsInRole("Admin"))
            {
                return Result<OrderDto>.Failure("You do not have permission to view this order");
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", request.OrderId);
            return Result<OrderDto>.Failure($"Failed to retrieve order: {ex.Message}");
        }
    }
}
