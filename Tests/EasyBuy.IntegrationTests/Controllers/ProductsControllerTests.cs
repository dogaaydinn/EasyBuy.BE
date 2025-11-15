using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EasyBuy.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact(Skip = "Requires database setup")]
    public async Task GetProducts_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/v1/products";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Requires authentication setup")]
    public async Task CreateProduct_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var url = "/api/v1/products";
        var newProduct = new
        {
            name = "Test Product",
            price = 99.99,
            quantity = 10,
            productType = 0,
            brand = "TestBrand"
        };

        // Act
        var response = await _client.PostAsJsonAsync(url, newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
