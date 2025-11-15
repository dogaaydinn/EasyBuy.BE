using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Reviews.DTOs;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IWriteRepository<Review> _reviewWriteRepository;
    private readonly IReadRepository<Review> _reviewReadRepository;
    private readonly IProductReadRepository _productRepository;
    private readonly IOrderReadRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IWriteRepository<Review> reviewWriteRepository,
        IReadRepository<Review> reviewReadRepository,
        IProductReadRepository productRepository,
        IOrderReadRepository orderRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _reviewWriteRepository = reviewWriteRepository;
        _reviewReadRepository = reviewReadRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<ReviewDto>.Failure("User not authenticated");
            }

            // Check if product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result<ReviewDto>.Failure($"Product {request.ProductId} not found");
            }

            // Check if user already reviewed this product
            var existingReviews = await _reviewReadRepository.GetAllAsync();
            var existingReview = existingReviews.FirstOrDefault(r =>
                r.ProductId == request.ProductId && r.AppUserId == userId);

            if (existingReview != null)
            {
                return Result<ReviewDto>.Failure("You have already reviewed this product");
            }

            // Check if user purchased this product (optional: for verified purchase badge)
            var userOrders = await _orderRepository.GetAllAsync();
            var hasPurchased = userOrders.Any(o =>
                o.AppUserId == userId &&
                o.OrderItems.Any(item => item.ProductId == request.ProductId));

            // Create review
            var review = new Review
            {
                ProductId = request.ProductId,
                Product = product,
                AppUserId = userId,
                AppUser = null!, // Will be loaded by EF Core
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                IsVerifiedPurchase = hasPurchased
            };

            await _reviewWriteRepository.AddAsync(review);

            _logger.LogInformation(
                "Review created for product {ProductId} by user {UserId}. Rating: {Rating}",
                request.ProductId, userId, request.Rating);

            var reviewDto = _mapper.Map<ReviewDto>(review);
            return Result<ReviewDto>.Success(reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return Result<ReviewDto>.Failure($"Failed to create review: {ex.Message}");
        }
    }
}
