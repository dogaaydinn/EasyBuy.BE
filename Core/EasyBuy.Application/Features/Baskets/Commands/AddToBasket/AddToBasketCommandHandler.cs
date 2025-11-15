using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Baskets.DTOs;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.AddToBasket;

public sealed class AddToBasketCommandHandler : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    private readonly IWriteRepository<Basket> _basketWriteRepository;
    private readonly IReadRepository<Basket> _basketReadRepository;
    private readonly IProductReadRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<AddToBasketCommandHandler> _logger;

    public AddToBasketCommandHandler(
        IWriteRepository<Basket> basketWriteRepository,
        IReadRepository<Basket> basketReadRepository,
        IProductReadRepository productRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<AddToBasketCommandHandler> logger)
    {
        _basketWriteRepository = basketWriteRepository;
        _basketReadRepository = basketReadRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<BasketDto>.Failure("User not authenticated");
            }

            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result<BasketDto>.Failure($"Product {request.ProductId} not found");
            }

            // Check stock
            if (product.Quantity < request.Quantity)
            {
                return Result<BasketDto>.Failure($"Insufficient stock. Available: {product.Quantity}");
            }

            // Get or create basket
            var baskets = await _basketReadRepository.GetAllAsync();
            var basket = baskets.FirstOrDefault(b => b.AppUserId == userId);

            if (basket == null)
            {
                basket = new Basket
                {
                    AppUserId = userId,
                    AppUser = null!, // Will be loaded by EF Core
                    BasketItems = new List<BasketItem>()
                };
                await _basketWriteRepository.AddAsync(basket);
            }

            // Check if product already in basket
            var existingItem = basket.BasketItems.FirstOrDefault(item => item.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (product.Quantity < newQuantity)
                {
                    return Result<BasketDto>.Failure($"Cannot add {request.Quantity} more. Available: {product.Quantity - existingItem.Quantity}");
                }
                existingItem.Quantity = newQuantity;
            }
            else
            {
                // Add new item
                var basketItem = new BasketItem
                {
                    BasketId = basket.Id,
                    Basket = basket,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = request.Quantity
                };
                basket.BasketItems.Add(basketItem);
            }

            await _basketWriteRepository.UpdateAsync(basket);

            _logger.LogInformation(
                "Added {Quantity} of product {ProductId} to basket for user {UserId}",
                request.Quantity, request.ProductId, userId);

            var basketDto = _mapper.Map<BasketDto>(basket);
            return Result<BasketDto>.Success(basketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to basket");
            return Result<BasketDto>.Failure($"Failed to add item to basket: {ex.Message}");
        }
    }
}
