using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace EasyBuy.Infrastructure.Services.Elasticsearch;

/// <summary>
/// Elasticsearch service implementation for full-text search.
/// Provides indexing and search capabilities for products.
/// </summary>
public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string ProductIndexName = "products";

    public ElasticsearchService(
        IElasticClient client,
        ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> IndexProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexDocumentAsync(product, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index product {ProductId}: {Error}",
                    product.Id, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogDebug("Successfully indexed product {ProductId}", product.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing product {ProductId}", product.Id);
            return false;
        }
    }

    public async Task<bool> BulkIndexProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        try
        {
            var productList = products.ToList();
            if (!productList.Any())
            {
                _logger.LogWarning("No products to index");
                return true;
            }

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(ProductIndexName)
                .IndexMany(productList), cancellationToken);

            if (!bulkResponse.IsValid)
            {
                _logger.LogError("Bulk indexing failed: {Error}",
                    bulkResponse.OriginalException?.Message ?? bulkResponse.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Successfully bulk indexed {Count} products", productList.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk indexing products");
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteAsync<Product>(productId, d => d
                .Index(ProductIndexName), cancellationToken);

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogError("Failed to delete product {ProductId}: {Error}",
                    productId, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogDebug("Successfully deleted product {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return false;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string query,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Index(ProductIndexName)
                .From(skip)
                .Size(take)
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields(f => f
                            .Field(p => p.Name, boost: 2.0)
                            .Field(p => p.Description)
                            .Field(p => p.Category.Name, boost: 1.5))
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(Fuzziness.Auto))), cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Search failed: {Error}",
                    searchResponse.OriginalException?.Message ?? searchResponse.ServerError?.ToString());
                return Enumerable.Empty<Product>();
            }

            _logger.LogDebug("Search for '{Query}' returned {Count} results", query, searchResponse.Documents.Count);
            return searchResponse.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", query);
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<IEnumerable<string>> SuggestProductsAsync(
        string partialName,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Index(ProductIndexName)
                .Size(maxSuggestions)
                .Query(q => q
                    .Match(m => m
                        .Field(p => p.Name)
                        .Query(partialName)
                        .Fuzziness(Fuzziness.Auto))), cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Suggestion search failed: {Error}",
                    searchResponse.OriginalException?.Message ?? searchResponse.ServerError?.ToString());
                return Enumerable.Empty<string>();
            }

            return searchResponse.Documents.Select(p => p.Name).Distinct();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for: {PartialName}", partialName);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pingResponse = await _client.PingAsync(ct: cancellationToken);
            return pingResponse.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch health check failed");
            return false;
        }
    }

    public async Task<bool> CreateProductIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(ProductIndexName, ct: cancellationToken);
            if (existsResponse.Exists)
            {
                _logger.LogInformation("Product index already exists");
                return true;
            }

            var createResponse = await _client.Indices.CreateAsync(ProductIndexName, c => c
                .Map<Product>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Name)
                            .Analyzer("standard")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))))
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("standard"))
                        .Keyword(k => k
                            .Name(n => n.Id))
                        .Number(n => n
                            .Name(nn => nn.Price)
                            .Type(NumberType.Double))
                        .Number(n => n
                            .Name(nn => nn.Stock)
                            .Type(NumberType.Integer))
                        .Object<Category>(o => o
                            .Name(n => n.Category)
                            .Properties(cp => cp
                                .Text(t => t
                                    .Name(c => c.Name)
                                    .Analyzer("standard")))))),
                cancellationToken);

            if (!createResponse.IsValid)
            {
                _logger.LogError("Failed to create product index: {Error}",
                    createResponse.OriginalException?.Message ?? createResponse.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Product index created successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product index");
            return false;
        }
    }

    public async Task<bool> DeleteProductIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(ProductIndexName, ct: cancellationToken);
            if (!existsResponse.Exists)
            {
                _logger.LogInformation("Product index does not exist");
                return true;
            }

            var deleteResponse = await _client.Indices.DeleteAsync(ProductIndexName, ct: cancellationToken);

            if (!deleteResponse.IsValid)
            {
                _logger.LogError("Failed to delete product index: {Error}",
                    deleteResponse.OriginalException?.Message ?? deleteResponse.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Product index deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product index");
            return false;
        }
    }
}
