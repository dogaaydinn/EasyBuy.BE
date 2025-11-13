using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Queries.GetBasket;

public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBasketQueryHandler> _logger;

    public GetBasketQueryHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetBasketQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<BasketDto>.Failure("User not authenticated");
            }

            var basket = await _context.Baskets
                .Include(b => b.BasketItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket == null)
            {
                // Return empty basket instead of error
                var emptyBasket = new BasketDto
                {
                    Id = Guid.Empty,
                    UserId = userId,
                    Items = new List<BasketItemDto>()
                };

                return Result<BasketDto>.Success(emptyBasket);
            }

            var basketDto = _mapper.Map<BasketDto>(basket);

            return Result<BasketDto>.Success(basketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket for user {UserId}", _currentUserService.UserId);
            return Result<BasketDto>.Failure($"An error occurred while retrieving basket: {ex.Message}");
        }
    }
}
