using EasyBuy.Domain.Entities;

namespace EasyBuy.Application.Common.Interfaces;

/// <summary>
/// Service for Elasticsearch full-text search operations.
/// Provides indexing and search capabilities for products and other entities.
/// </summary>
public interface IElasticsearchService
{
    /// <summary>
    /// Index a single product in Elasticsearch.
    /// </summary>
    Task<bool> IndexProductAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Index multiple products in bulk.
    /// </summary>
    Task<bool> BulkIndexProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a product from the index.
    /// </summary>
    Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search products by full-text query.
    /// Searches across name, description, and category.
    /// </summary>
    Task<IEnumerable<Product>> SearchProductsAsync(
        string query,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggest product names based on partial input (autocomplete).
    /// </summary>
    Task<IEnumerable<string>> SuggestProductsAsync(
        string partialName,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if Elasticsearch is available and healthy.
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update the product index with proper mappings.
    /// </summary>
    Task<bool> CreateProductIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete the product index.
    /// </summary>
    Task<bool> DeleteProductIndexAsync(CancellationToken cancellationToken = default);
}
