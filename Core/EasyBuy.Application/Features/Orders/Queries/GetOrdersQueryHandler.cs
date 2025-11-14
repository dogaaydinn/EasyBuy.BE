using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.DTOs.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries;

/// <summary>
/// Handler for GetOrdersQuery.
/// Retrieves orders with pagination, filtering, and sorting.
/// </summary>
public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderDto>>>
{
    private readonly IOrderReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(
        IOrderReadRepository repository,
        IMapper mapper,
        ILogger<GetOrdersQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting orders: Page={Page}, Size={Size}, Status={Status}",
            request.PageNumber, request.PageSize, request.Status);

        try
        {
            // Get orders with filtering
            var orders = await _repository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Status))
            {
                orders = orders.Where(o => o.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (request.UserId.HasValue)
            {
                orders = orders.Where(o => o.UserId == request.UserId.Value);
            }

            // Apply sorting
            orders = request.OrderBy?.ToLower() switch
            {
                "totalamount" => request.Descending
                    ? orders.OrderByDescending(o => o.TotalAmount)
                    : orders.OrderBy(o => o.TotalAmount),
                "status" => request.Descending
                    ? orders.OrderByDescending(o => o.Status)
                    : orders.OrderBy(o => o.Status),
                _ => request.Descending
                    ? orders.OrderByDescending(o => o.OrderDate)
                    : orders.OrderBy(o => o.OrderDate)
            };

            // Get total count
            var totalCount = orders.Count();

            // Apply pagination
            var pagedOrders = orders
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders);

            var pagedResult = new PagedResult<OrderDto>(
                orderDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} orders out of {Total}",
                orderDtos.Count, totalCount);

            return Result<PagedResult<OrderDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return Result<PagedResult<OrderDto>>.Failure($"Failed to retrieve orders: {ex.Message}");
        }
    }
}
