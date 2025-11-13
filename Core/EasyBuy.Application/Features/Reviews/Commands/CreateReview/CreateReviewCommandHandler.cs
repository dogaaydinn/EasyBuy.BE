using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<ReviewDto>.Failure("User not authenticated");
            }

            // Check if product exists
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (product == null)
            {
                return Result<ReviewDto>.Failure("Product not found");
            }

            // Check if user has purchased this product
            var hasPurchased = await _context.Orders
                .AnyAsync(o => o.UserId == userId
                    && o.Status == Domain.Enums.OrderStatus.Delivered
                    && o.OrderItems.Any(oi => oi.ProductId == request.ProductId),
                    cancellationToken);

            if (!hasPurchased)
            {
                return Result<ReviewDto>.Failure("You can only review products you have purchased");
            }

            // Check if user already reviewed this product
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.UserId == userId, cancellationToken);

            if (existingReview != null)
            {
                return Result<ReviewDto>.Failure("You have already reviewed this product. You can update your existing review instead.");
            }

            // Create review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(review, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review created for product {ProductId} by user {UserId}", request.ProductId, userId);

            // Reload with user data for mapping
            review = await _context.Reviews
                .Include(r => r.User)
                .FirstAsync(r => r.Id == review.Id, cancellationToken);

            var reviewDto = _mapper.Map<ReviewDto>(review);

            return Result<ReviewDto>.Success(reviewDto, "Review created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for product {ProductId}", request.ProductId);
            return Result<ReviewDto>.Failure($"An error occurred while creating the review: {ex.Message}");
        }
    }
}
