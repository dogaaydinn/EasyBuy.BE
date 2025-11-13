using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.AddToBasket;

public class AddToBasketCommandHandler : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<AddToBasketCommandHandler> _logger;

    public AddToBasketCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<AddToBasketCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<BasketDto>.Failure("User not authenticated");
            }

            // Check if product exists and is available
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (product == null)
            {
                return Result<BasketDto>.Failure("Product not found or is inactive");
            }

            // Check stock availability
            if (product.Stock < request.Quantity)
            {
                return Result<BasketDto>.Failure($"Insufficient stock. Available: {product.Stock}, Requested: {request.Quantity}");
            }

            // Get or create basket for user
            var basket = await _context.Baskets
                .Include(b => b.BasketItems)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket == null)
            {
                basket = new Basket
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _context.Baskets.AddAsync(basket, cancellationToken);
            }

            // Check if product already in basket
            var existingItem = basket.BasketItems.FirstOrDefault(bi => bi.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Quantity;

                if (product.Stock < newQuantity)
                {
                    return Result<BasketDto>.Failure($"Insufficient stock. Available: {product.Stock}, Total requested: {newQuantity}");
                }

                existingItem.Quantity = newQuantity;
                existingItem.ModifiedDate = DateTime.UtcNow;

                _logger.LogInformation("Updated quantity for product {ProductId} in basket. New quantity: {Quantity}",
                    product.Id, newQuantity);
            }
            else
            {
                // Add new item to basket
                var basketItem = new BasketItem
                {
                    Id = Guid.NewGuid(),
                    BasketId = basket.Id,
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    Price = product.Price,
                    CreatedDate = DateTime.UtcNow
                };

                basket.BasketItems.Add(basketItem);

                _logger.LogInformation("Added product {ProductId} to basket with quantity {Quantity}",
                    product.Id, request.Quantity);
            }

            basket.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Reload basket with all related data for mapping
            basket = await _context.Baskets
                .Include(b => b.BasketItems)
                    .ThenInclude(bi => bi.Product)
                .FirstAsync(b => b.Id == basket.Id, cancellationToken);

            var basketDto = _mapper.Map<BasketDto>(basket);

            return Result<BasketDto>.Success(basketDto, "Product added to basket successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to basket", request.ProductId);
            return Result<BasketDto>.Failure($"An error occurred while adding product to basket: {ex.Message}");
        }
    }
}
