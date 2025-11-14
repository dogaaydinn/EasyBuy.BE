using EasyBuy.Application.Contracts.Category;
using EasyBuy.Application.Contracts.Product;
using EasyBuy.Application.Contracts.Review;
using EasyBuy.Application.Features.Categories.Commands.CreateCategory;
using EasyBuy.Application.Features.Categories.Commands.DeleteCategory;
using EasyBuy.Application.Features.Categories.Commands.UpdateCategory;
using EasyBuy.Application.Features.Products.Commands.CreateProduct;
using EasyBuy.Application.Features.Products.Commands.DeleteProduct;
using EasyBuy.Application.Features.Products.Commands.UpdateProduct;
using EasyBuy.Application.Features.Reviews.Commands.CreateReview;
using EasyBuy.Application.Features.Reviews.Commands.DeleteReview;
using EasyBuy.Application.Features.Reviews.Commands.UpdateReview;
using HotChocolate.Authorization;
using MediatR;

namespace EasyBuy.WebAPI.GraphQL.Mutations;

/// <summary>
/// Root GraphQL Mutation type.
/// Provides write operations for all entities using CQRS pattern with MediatR.
/// All mutations require authentication.
/// </summary>
[Authorize]
public class Mutation
{
    // ====================================================================
    // PRODUCT MUTATIONS
    // ====================================================================

    /// <summary>
    /// Create a new product.
    /// Example:
    /// mutation {
    ///   createProduct(input: {
    ///     name: "New Product"
    ///     description: "Description"
    ///     price: 99.99
    ///     stock: 100
    ///     categoryId: "guid"
    ///   }) {
    ///     id
    ///     name
    ///     price
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageProducts")]
    public async Task<CreateProductResponse> CreateProductAsync(
        CreateProductDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand { ProductDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Update an existing product.
    /// Example:
    /// mutation {
    ///   updateProduct(id: "guid", input: {
    ///     name: "Updated Name"
    ///     price: 89.99
    ///   }) {
    ///     id
    ///     name
    ///     price
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageProducts")]
    public async Task<UpdateProductResponse> UpdateProductAsync(
        Guid id,
        UpdateProductDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand { Id = id, ProductDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Delete a product by ID.
    /// Example:
    /// mutation {
    ///   deleteProduct(id: "guid") {
    ///     success
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageProducts")]
    public async Task<DeleteProductResponse> DeleteProductAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand { Id = id };
        return await mediator.Send(command, cancellationToken);
    }

    // ====================================================================
    // CATEGORY MUTATIONS
    // ====================================================================

    /// <summary>
    /// Create a new category.
    /// Example:
    /// mutation {
    ///   createCategory(input: {
    ///     name: "New Category"
    ///     description: "Description"
    ///     parentCategoryId: "guid"
    ///   }) {
    ///     id
    ///     name
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageCategories")]
    public async Task<CreateCategoryResponse> CreateCategoryAsync(
        CreateCategoryDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand { CategoryDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Update an existing category.
    /// Example:
    /// mutation {
    ///   updateCategory(id: "guid", input: {
    ///     name: "Updated Category"
    ///     description: "New description"
    ///   }) {
    ///     id
    ///     name
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageCategories")]
    public async Task<UpdateCategoryResponse> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand { Id = id, CategoryDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Delete a category by ID.
    /// Example:
    /// mutation {
    ///   deleteCategory(id: "guid") {
    ///     success
    ///   }
    /// }
    /// </summary>
    [Authorize(Policy = "ManageCategories")]
    public async Task<DeleteCategoryResponse> DeleteCategoryAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand { Id = id };
        return await mediator.Send(command, cancellationToken);
    }

    // ====================================================================
    // REVIEW MUTATIONS
    // Reviews can be created by any authenticated user (handled by class-level [Authorize])
    // Update/Delete use ManageReviews policy (users own reviews + Admin/Manager moderation)
    // ====================================================================

    /// <summary>
    /// Create a new review.
    /// Example:
    /// mutation {
    ///   createReview(input: {
    ///     productId: "guid"
    ///     rating: 5
    ///     comment: "Great product!"
    ///   }) {
    ///     id
    ///     rating
    ///     comment
    ///   }
    /// }
    /// </summary>
    public async Task<CreateReviewResponse> CreateReviewAsync(
        CreateReviewDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand { ReviewDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Update an existing review.
    /// Example:
    /// mutation {
    ///   updateReview(id: "guid", input: {
    ///     rating: 4
    ///     comment: "Updated review"
    ///   }) {
    ///     id
    ///     rating
    ///     comment
    ///   }
    /// }
    /// </summary>
    public async Task<UpdateReviewResponse> UpdateReviewAsync(
        Guid id,
        UpdateReviewDto input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand { Id = id, ReviewDto = input };
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Delete a review by ID.
    /// Example:
    /// mutation {
    ///   deleteReview(id: "guid") {
    ///     success
    ///   }
    /// }
    /// </summary>
    public async Task<DeleteReviewResponse> DeleteReviewAsync(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand { Id = id };
        return await mediator.Send(command, cancellationToken);
    }
}
