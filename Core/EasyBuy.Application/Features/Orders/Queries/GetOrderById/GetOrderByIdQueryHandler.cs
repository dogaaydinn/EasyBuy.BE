using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Manager");

            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.User)
                .Where(o => o.Id == request.Id && !o.IsDeleted);

            // Regular users can only view their own orders
            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId);
            }

            var order = await query.FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return Result<OrderDto>.Failure("Order not found or you don't have permission to view it");
            }

            var orderDto = _mapper.Map<OrderDto>(order);

            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", request.Id);
            return Result<OrderDto>.Failure($"An error occurred while retrieving the order: {ex.Message}");
        }
    }
}
