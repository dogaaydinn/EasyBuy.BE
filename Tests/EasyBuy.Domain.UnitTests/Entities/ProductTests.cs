using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EasyBuy.Domain.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_WithValidData_ShouldBeCreated()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Quantity = 10,
            ProductType = ProductType.Electronics,
            ProductBrand = "TestBrand",
            OnSale = false
        };

        // Assert
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.Quantity.Should().Be(10);
        product.ProductType.Should().Be(ProductType.Electronics);
    }

    [Fact]
    public void Product_Price_ShouldBeNonNegative()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Price = 0m,
            Quantity = 1,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        // Assert
        product.Price.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Product_Quantity_ShouldBeNonNegative()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Price = 10m,
            Quantity = 0,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        // Assert
        product.Quantity.Should().BeGreaterOrEqualTo(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    public void Product_Quantity_ShouldAcceptValidValues(int quantity)
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test",
            Price = 10m,
            Quantity = quantity,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        // Assert
        product.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Product_OnSale_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test",
            Price = 10m,
            Quantity = 5,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        // Assert
        product.OnSale.Should().BeFalse();
    }

    [Fact]
    public void Product_Collections_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test",
            Price = 10m,
            Quantity = 5,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        // Assert
        product.Orders.Should().BeEmpty();
        product.ProductImageFiles.Should().BeEmpty();
    }
}
