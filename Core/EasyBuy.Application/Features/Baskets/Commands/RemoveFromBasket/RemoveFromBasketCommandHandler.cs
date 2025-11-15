using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Baskets.DTOs;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;

public sealed class RemoveFromBasketCommandHandler : IRequestHandler<RemoveFromBasketCommand, Result<BasketDto>>
{
    private readonly IWriteRepository<Basket> _basketWriteRepository;
    private readonly IReadRepository<Basket> _basketReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<RemoveFromBasketCommandHandler> _logger;

    public RemoveFromBasketCommandHandler(
        IWriteRepository<Basket> basketWriteRepository,
        IReadRepository<Basket> basketReadRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<RemoveFromBasketCommandHandler> logger)
    {
        _basketWriteRepository = basketWriteRepository;
        _basketReadRepository = basketReadRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(RemoveFromBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<BasketDto>.Failure("User not authenticated");
            }

            var baskets = await _basketReadRepository.GetAllAsync();
            var basket = baskets.FirstOrDefault(b => b.AppUserId == userId);

            if (basket == null)
            {
                return Result<BasketDto>.Failure("Basket not found");
            }

            var item = basket.BasketItems.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (item == null)
            {
                return Result<BasketDto>.Failure("Item not found in basket");
            }

            basket.BasketItems.Remove(item);
            await _basketWriteRepository.UpdateAsync(basket);

            _logger.LogInformation(
                "Removed product {ProductId} from basket for user {UserId}",
                request.ProductId, userId);

            var basketDto = _mapper.Map<BasketDto>(basket);
            return Result<BasketDto>.Success(basketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from basket");
            return Result<BasketDto>.Failure($"Failed to remove item from basket: {ex.Message}");
        }
    }
}
