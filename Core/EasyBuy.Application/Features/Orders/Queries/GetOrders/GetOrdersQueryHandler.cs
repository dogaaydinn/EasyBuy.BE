using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderListDto>>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetOrdersQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrderListDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Manager");

            // Start with base query
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => !o.IsDeleted)
                .AsQueryable();

            // Filter by user - regular users can only see their own orders
            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId);
            }
            else if (request.UserId.HasValue)
            {
                // Admin can filter by specific user
                query = query.Where(o => o.UserId == request.UserId.Value);
            }

            // Apply filters
            if (request.Status.HasValue)
            {
                query = query.Where(o => o.Status == request.Status.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= request.ToDate.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "ordernumber" => request.SortDescending
                    ? query.OrderByDescending(o => o.OrderNumber)
                    : query.OrderBy(o => o.OrderNumber),
                "total" => request.SortDescending
                    ? query.OrderByDescending(o => o.Total)
                    : query.OrderBy(o => o.Total),
                "status" => request.SortDescending
                    ? query.OrderByDescending(o => o.Status)
                    : query.OrderBy(o => o.Status),
                _ => request.SortDescending
                    ? query.OrderByDescending(o => o.OrderDate)
                    : query.OrderBy(o => o.OrderDate)
            };

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var orders = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<OrderListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<OrderListDto>
            {
                Items = orders,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<OrderListDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return Result<PagedResult<OrderListDto>>.Failure($"An error occurred while retrieving orders: {ex.Message}");
        }
    }
}
