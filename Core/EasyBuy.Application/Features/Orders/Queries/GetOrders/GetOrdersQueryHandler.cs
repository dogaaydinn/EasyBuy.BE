using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Orders.DTOs;
using EasyBuy.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderDto>>>
{
    private readonly IOrderReadRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(
        IOrderReadRepository orderRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all orders
            var orders = await _orderRepository.GetAllAsync();

            // Apply filters
            var query = orders.AsQueryable();

            // Filter by user if specified (admin can see all, regular users only their own)
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(o => o.AppUserId == request.UserId);
            }
            else if (!_currentUserService.IsInRole("Admin"))
            {
                // Non-admin users can only see their own orders
                var userId = _currentUserService.UserId;
                query = query.Where(o => o.AppUserId == userId);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= request.ToDate.Value);
            }

            // Sort by order date (newest first)
            query = query.OrderByDescending(o => o.OrderDate);

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var pagedOrders = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders);

            var pagedResult = new PagedResult<OrderDto>(
                orderDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagedResult<OrderDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return Result<PagedResult<OrderDto>>.Failure($"Failed to retrieve orders: {ex.Message}");
        }
    }
}
