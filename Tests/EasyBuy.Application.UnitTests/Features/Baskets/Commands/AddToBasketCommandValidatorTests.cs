using EasyBuy.Application.Features.Baskets.Commands.AddToBasket;
using FluentAssertions;
using Xunit;

namespace EasyBuy.Application.UnitTests.Features.Baskets.Commands;

public class AddToBasketCommandValidatorTests
{
    private readonly AddToBasketCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 5
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyProductId_ShouldFail()
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.Empty,
            Quantity = 1
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithZeroOrNegativeQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = quantity
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_WithQuantityExceeding100_ShouldFail()
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 101
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithValidQuantity_ShouldPass(int quantity)
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = quantity
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
