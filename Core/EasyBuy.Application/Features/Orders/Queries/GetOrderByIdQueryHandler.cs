using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.DTOs.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries;

/// <summary>
/// Handler for GetOrderByIdQuery.
/// Retrieves order with caching support.
/// </summary>
public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderReadRepository _repository;
    private readonly ILayeredCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderReadRepository repository,
        ILayeredCacheService cache,
        IMapper mapper,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order by ID: {OrderId}", request.OrderId);

        try
        {
            // Try cache first
            var cacheKey = $"order:{request.OrderId}";
            var orderDto = await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var order = await _repository.GetByIdAsync(request.OrderId);
                    if (order == null)
                    {
                        return null!;
                    }

                    return _mapper.Map<OrderDto>(order);
                },
                TimeSpan.FromMinutes(10),
                cancellationToken);

            if (orderDto == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", request.OrderId);
                return Result<OrderDto>.Failure($"Order not found: {request.OrderId}");
            }

            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order: {OrderId}", request.OrderId);
            return Result<OrderDto>.Failure($"Failed to retrieve order: {ex.Message}");
        }
    }
}
