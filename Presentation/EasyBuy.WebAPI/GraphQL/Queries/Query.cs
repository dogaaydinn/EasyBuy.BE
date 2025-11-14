using EasyBuy.Domain.Entities;
using EasyBuy.Infrastructure.Persistence;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.WebAPI.GraphQL.Queries;

/// <summary>
/// Root GraphQL Query type.
/// Provides read-only access to all entities with filtering, sorting, and pagination.
/// </summary>
public class Query
{
    /// <summary>
    /// Get all products with optional filtering, sorting, and pagination.
    /// Example: { products(where: { price: { gt: 50 } }) { id name price } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(
        [ScopedService] ApplicationDbContext context)
    {
        return context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews);
    }

    /// <summary>
    /// Get a single product by ID.
    /// Example: { product(id: "guid") { id name price category { name } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    public async Task<Product?> GetProductAsync(
        Guid id,
        [ScopedService] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all categories with optional filtering, sorting, and pagination.
    /// Example: { categories(order: { name: ASC }) { id name parentCategory { name } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> GetCategories(
        [ScopedService] ApplicationDbContext context)
    {
        return context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products);
    }

    /// <summary>
    /// Get a single category by ID.
    /// Example: { category(id: "guid") { id name products { name } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    public async Task<Category?> GetCategoryAsync(
        Guid id,
        [ScopedService] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all reviews with optional filtering, sorting, and pagination.
    /// Example: { reviews(where: { rating: { gte: 4 } }) { rating comment product { name } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Review> GetReviews(
        [ScopedService] ApplicationDbContext context)
    {
        return context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User);
    }

    /// <summary>
    /// Get a single review by ID.
    /// Example: { review(id: "guid") { rating comment product { name } user { firstName } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    public async Task<Review?> GetReviewAsync(
        Guid id,
        [ScopedService] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all orders with optional filtering, sorting, and pagination.
    /// Example: { orders(where: { status: COMPLETED }) { orderNumber totalAmount status } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> GetOrders(
        [ScopedService] ApplicationDbContext context)
    {
        return context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User);
    }

    /// <summary>
    /// Get a single order by ID.
    /// Example: { order(id: "guid") { orderNumber totalAmount orderItems { productName quantity } } }
    /// </summary>
    [UseDbContext(typeof(ApplicationDbContext))]
    public async Task<Order?> GetOrderAsync(
        Guid id,
        [ScopedService] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
