using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Repositories.Category;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Handler for CreateCategoryCommand.
/// Creates a new category with optional parent-child relationship.
/// </summary>
public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryWriteRepository _writeRepository;
    private readonly ICategoryReadRepository _readRepository;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICategoryWriteRepository writeRepository,
        ICategoryReadRepository readRepository,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category: {CategoryName}, Parent: {ParentId}", 
            request.Name, request.ParentCategoryId);

        try
        {
            // Validate parent category exists if specified
            if (request.ParentCategoryId.HasValue)
            {
                var parentCategory = await _readRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    return Result<Guid>.Failure($"Parent category not found: {request.ParentCategoryId}");
                }

                if (!parentCategory.IsActive)
                {
                    return Result<Guid>.Failure("Cannot add subcategory to an inactive parent category");
                }
            }

            // Check for duplicate category name at the same level
            var existingCategories = await _readRepository.GetAllAsync();
            var duplicate = existingCategories.FirstOrDefault(c => 
                c.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && 
                c.ParentCategoryId == request.ParentCategoryId);

            if (duplicate != null)
            {
                return Result<Guid>.Failure($"Category with name '{request.Name}' already exists at this level");
            }

            // Create category
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ParentCategoryId = request.ParentCategoryId,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };

            await _writeRepository.AddAsync(category);

            _logger.LogInformation("Category created successfully: {CategoryId}, Name: {CategoryName}", 
                category.Id, category.Name);

            return Result<Guid>.Success(category.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category: {CategoryName}", request.Name);
            return Result<Guid>.Failure($"Failed to create category: {ex.Message}");
        }
    }
}
