using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Baskets.DTOs;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Queries.GetBasket;

public sealed class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    private readonly IReadRepository<Basket> _basketRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBasketQueryHandler> _logger;

    public GetBasketQueryHandler(
        IReadRepository<Basket> basketRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetBasketQueryHandler> logger)
    {
        _basketRepository = basketRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<BasketDto>.Failure("User not authenticated");
            }

            var baskets = await _basketRepository.GetAllAsync();
            var basket = baskets.FirstOrDefault(b => b.AppUserId == userId);

            if (basket == null)
            {
                // Return empty basket
                return Result<BasketDto>.Success(new BasketDto
                {
                    UserId = userId,
                    Items = new List<BasketItemDto>()
                });
            }

            var basketDto = _mapper.Map<BasketDto>(basket);
            return Result<BasketDto>.Success(basketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket");
            return Result<BasketDto>.Failure($"Failed to retrieve basket: {ex.Message}");
        }
    }
}
