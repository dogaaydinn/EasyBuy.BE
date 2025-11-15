using EasyBuy.Application.Features.Orders.Commands.CreateOrder;
using EasyBuy.Application.Features.Orders.DTOs;
using FluentAssertions;
using Xunit;

namespace EasyBuy.Application.UnitTests.Features.Orders.Commands;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyItems_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>(),
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithInvalidQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = quantity }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Theory]
    [InlineData("", "Must not be empty")]
    [InlineData("X", "Cannot exceed 50 characters")]
    public void Validate_WithInvalidFirstName_ShouldFail(string firstName, string expectedMessage)
    {
        // Arrange
        var longName = new string('X', 51);
        var actualFirstName = firstName == "X" ? longName : firstName;

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = actualFirstName,
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FirstName"));
    }

    [Theory]
    [InlineData("12345", true)]      // Valid 5-digit
    [InlineData("12345-6789", true)]  // Valid ZIP+4
    [InlineData("1234", false)]       // Too short
    [InlineData("ABCDE", false)]      // Letters
    [InlineData("12345-678", false)]  // Incomplete ZIP+4
    public void Validate_ZipCode_ShouldValidateFormat(string zipCode, bool shouldBeValid)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = zipCode,
                Country = "USA"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        if (shouldBeValid)
        {
            result.IsValid.Should().BeTrue();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("ZipCode"));
        }
    }

    [Fact]
    public void Validate_WithMissingAddress_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = null!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ShippingAddress");
    }

    [Fact]
    public void Validate_WithLongNotes_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            },
            Notes = new string('X', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }
}
