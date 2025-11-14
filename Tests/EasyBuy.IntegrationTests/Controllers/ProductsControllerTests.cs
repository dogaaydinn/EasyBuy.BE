using System.Net;
using System.Net.Http.Json;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.IntegrationTests.Infrastructure;

namespace EasyBuy.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ProductsController.
/// Tests the full request pipeline from HTTP to database.
/// </summary>
public class ProductsControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.HttpClient;
    }

    [Fact]
    public async Task GetProducts_WhenCalled_ReturnsOkWithProducts()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var createDto = new CreateProductDto
        {
            Name = "Integration Test Product",
            Description = "Created via integration test",
            Price = 99.99m,
            Stock = 100,
            CategoryId = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(createDto.Name);
        createdProduct.Price.Should().Be(createDto.Price);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var invalidDto = new CreateProductDto
        {
            Name = "", // Invalid: empty name
            Description = "Test",
            Price = -10, // Invalid: negative price
            Stock = -5   // Invalid: negative stock
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_WhenExists_ReturnsProduct()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // First create a product
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 49.99m,
            Stock = 50
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(createdProduct.Id);
        product.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task GetProductById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsNoContent()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Create product first
        var createDto = new CreateProductDto
        {
            Name = "Original Name",
            Description = "Original Description",
            Price = 99.99m,
            Stock = 100
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 149.99m,
            Stock = 150
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{createdProduct!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/v1/products/{createdProduct.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        updatedProduct!.Name.Should().Be(updateDto.Name);
        updatedProduct.Price.Should().Be(updateDto.Price);
    }

    [Fact]
    public async Task DeleteProduct_WhenExists_ReturnsNoContent()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var createDto = new CreateProductDto
        {
            Name = "To Be Deleted",
            Description = "This product will be deleted",
            Price = 99.99m,
            Stock = 10
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{createdProduct!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/v1/products/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Create multiple products
        for (int i = 1; i <= 25; i++)
        {
            var dto = new CreateProductDto
            {
                Name = $"Product {i}",
                Description = $"Description {i}",
                Price = i * 10,
                Stock = i * 5
            };
            await _client.PostAsJsonAsync("/api/v1/products", dto);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/products?pageNumber=2&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
        products!.Count.Should().BeLessOrEqualTo(10);
    }

    [Fact]
    public async Task ConcurrentCreates_ShouldAllSucceed()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            var dto = new CreateProductDto
            {
                Name = $"Concurrent Product {i}",
                Description = $"Created concurrently {i}",
                Price = i * 10,
                Stock = i * 5
            };

            return await _client.PostAsJsonAsync("/api/v1/products", dto);
        });

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));

        // Verify all products were created
        var getAllResponse = await _client.GetAsync("/api/v1/products");
        var allProducts = await getAllResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        allProducts.Should().HaveCount(10);
    }
}
