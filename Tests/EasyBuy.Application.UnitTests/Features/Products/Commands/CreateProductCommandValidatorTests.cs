using EasyBuy.Application.Features.Products.Commands.CreateProduct;
using FluentAssertions;
using Xunit;

namespace EasyBuy.Application.UnitTests.Features.Products.Commands;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Quantity = 10,
            ProductType = 0, // Electronics
            Brand = "TestBrand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyName_ShouldFail(string name)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = name,
            Price = 10m,
            Quantity = 5,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = new string('X', 201),
            Price = 10m,
            Quantity = 5,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativePrice_ShouldFail(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Product",
            Price = price,
            Quantity = 5,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNegativeQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Product",
            Price = 10m,
            Quantity = quantity,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Product",
            Description = new string('X', 1001),
            Price = 10m,
            Quantity = 5,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithValidDescription_ShouldPass()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Product",
            Description = "Valid description with reasonable length",
            Price = 10m,
            Quantity = 5,
            ProductType = 0,
            Brand = "Brand"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
