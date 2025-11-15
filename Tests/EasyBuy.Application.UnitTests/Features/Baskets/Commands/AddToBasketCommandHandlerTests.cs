using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Baskets.Commands.AddToBasket;
using EasyBuy.Application.Features.Baskets.DTOs;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyBuy.Application.UnitTests.Features.Baskets.Commands;

public class AddToBasketCommandHandlerTests
{
    private readonly Mock<IWriteRepository<Basket>> _basketWriteRepositoryMock;
    private readonly Mock<IReadRepository<Basket>> _basketReadRepositoryMock;
    private readonly Mock<IProductReadRepository> _productRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AddToBasketCommandHandler>> _loggerMock;
    private readonly AddToBasketCommandHandler _handler;

    public AddToBasketCommandHandlerTests()
    {
        _basketWriteRepositoryMock = new Mock<IWriteRepository<Basket>>();
        _basketReadRepositoryMock = new Mock<IReadRepository<Basket>>();
        _productRepositoryMock = new Mock<IProductReadRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AddToBasketCommandHandler>>();

        _handler = new AddToBasketCommandHandler(
            _basketWriteRepositoryMock.Object,
            _basketReadRepositoryMock.Object,
            _productRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not authenticated");
    }

    [Fact]
    public async Task Handle_WithNonexistentProduct_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var command = new AddToBasketCommand
        {
            ProductId = productId,
            Quantity = 1
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
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 10m,
            Quantity = 2,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var command = new AddToBasketCommand
        {
            ProductId = productId,
            Quantity = 5 // More than available
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_WithNoExistingBasket_ShouldCreateNewBasket()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = "user-123";
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 10m,
            Quantity = 10,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _basketReadRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Basket>()); // No existing basket
        _mapperMock.Setup(x => x.Map<BasketDto>(It.IsAny<Basket>()))
            .Returns(new BasketDto());

        var command = new AddToBasketCommand
        {
            ProductId = productId,
            Quantity = 2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _basketWriteRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Basket>(b => b.AppUserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingItemInBasket_ShouldIncrementQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = "user-123";
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 10m,
            Quantity = 10,
            ProductType = ProductType.Electronics,
            ProductBrand = "Brand"
        };

        var existingBasketItem = new BasketItem
        {
            ProductId = productId,
            Product = product,
            Quantity = 2
        };

        var existingBasket = new Basket
        {
            AppUserId = userId,
            BasketItems = new List<BasketItem> { existingBasketItem }
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _basketReadRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Basket> { existingBasket });
        _mapperMock.Setup(x => x.Map<BasketDto>(It.IsAny<Basket>()))
            .Returns(new BasketDto());

        var command = new AddToBasketCommand
        {
            ProductId = productId,
            Quantity = 3
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingBasketItem.Quantity.Should().Be(5); // 2 + 3
        _basketWriteRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Basket>()),
            Times.Once);
    }
}
