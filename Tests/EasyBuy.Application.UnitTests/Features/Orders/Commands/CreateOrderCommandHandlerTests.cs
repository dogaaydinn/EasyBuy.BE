using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Orders.Commands.CreateOrder;
using EasyBuy.Application.Features.Orders.DTOs;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyBuy.Application.UnitTests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepositoryMock;
    private readonly Mock<IProductReadRepository> _productReadRepositoryMock;
    private readonly Mock<IReadRepository<Delivery>> _deliveryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderWriteRepositoryMock = new Mock<IOrderWriteRepository>();
        _productReadRepositoryMock = new Mock<IProductReadRepository>();
        _deliveryRepositoryMock = new Mock<IReadRepository<Delivery>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

        _handler = new CreateOrderCommandHandler(
            _orderWriteRepositoryMock.Object,
            _productReadRepositoryMock.Object,
            _deliveryRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not authenticated");
    }

    [Fact]
    public async Task Handle_WithInvalidDeliveryMethod_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _deliveryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Delivery?)null);

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid delivery method");
    }

    [Fact]
    public async Task Handle_WithNonexistentProduct_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _deliveryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateTestDelivery());
        _productReadRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, quantity: 5);

        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _deliveryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateTestDelivery());
        _productReadRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 10 } // Request more than available
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, quantity: 10);
        var userId = "user-123";

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.UserEmail).Returns("test@example.com");
        _deliveryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateTestDelivery());
        _productReadRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mapperMock.Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto { Id = Guid.NewGuid() });

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 2 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verify order was saved
        _orderWriteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>()),
            Times.Once);

        // Verify inventory was reduced
        product.Quantity.Should().Be(8); // 10 - 2
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, price: 100m, quantity: 10);
        var delivery = CreateTestDelivery(price: 10m);

        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _currentUserServiceMock.Setup(x => x.UserEmail).Returns("test@example.com");
        _deliveryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(delivery);
        _productReadRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        Order? capturedOrder = null;
        _orderWriteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto());

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 2 }
            },
            DeliveryMethodId = Guid.NewGuid(),
            ShippingAddress = CreateTestAddress()
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Subtotal.Should().Be(200m); // 2 * 100
        capturedOrder.TaxAmount.Should().Be(20m); // 10% of 200
        capturedOrder.ShippingCost.Should().Be(10m);
        capturedOrder.Total.Should().Be(230m); // 200 + 20 + 10
    }

    private static AddressDto CreateTestAddress()
    {
        return new AddressDto
        {
            FirstName = "John",
            LastName = "Doe",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            Country = "USA"
        };
    }

    private static Product CreateTestProduct(Guid id, decimal price = 100m, int quantity = 10)
    {
        return new Product
        {
            Id = id,
            Name = "Test Product",
            Price = price,
            Quantity = quantity,
            ProductType = ProductType.Electronics,
            ProductBrand = "TestBrand"
        };
    }

    private static Delivery CreateTestDelivery(decimal price = 5m)
    {
        return new Delivery
        {
            Id = Guid.NewGuid(),
            DeliveryMethod = "Standard",
            ShortName = "STD",
            DeliveryTime = "3-5 days",
            Description = "Standard delivery",
            Price = price
        };
    }
}
