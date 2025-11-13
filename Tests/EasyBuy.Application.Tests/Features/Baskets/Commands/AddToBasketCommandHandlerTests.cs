using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Features.Baskets.Commands.AddToBasket;
using EasyBuy.Application.Mappings;
using EasyBuy.Application.Tests.Helpers;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Tests.Features.Baskets.Commands;

public class AddToBasketCommandHandlerTests : IDisposable
{
    private readonly EasyBuyDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AddToBasketCommandHandler>> _mockLogger;
    private readonly AddToBasketCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public AddToBasketCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<AddToBasketCommandHandler>>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _handler = new AddToBasketCommandHandler(
            _context,
            _mockCurrentUserService.Object,
            _mapper,
            _mockLogger.Object);

        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);
    }

    [Fact]
    public async Task Handle_NewProduct_ShouldAddToBasket()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 50m,
            Stock = 10,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new AddToBasketCommand
        {
            ProductId = product.Id,
            Quantity = 2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].ProductId.Should().Be(product.Id);
        result.Data.Items[0].Quantity.Should().Be(2);
        result.Data.SubTotal.Should().Be(100m);

        var basket = _context.Baskets.FirstOrDefault();
        basket.Should().NotBeNull();
        basket!.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 50m,
            Stock = 10,
            IsActive = true
        };

        var basket = new Basket
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Quantity = 1,
            Price = product.Price
        };

        basket.BasketItems.Add(basketItem);

        _context.Products.Add(product);
        _context.Baskets.Add(basket);
        await _context.SaveChangesAsync();

        var command = new AddToBasketCommand
        {
            ProductId = product.Id,
            Quantity = 2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Quantity.Should().Be(3); // 1 + 2

        var updatedBasketItem = _context.BasketItems.FirstOrDefault();
        updatedBasketItem!.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 50m,
            Stock = 2,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new AddToBasketCommand
        {
            ProductId = product.Id,
            Quantity = 5
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_InactiveProduct_ShouldReturnFailure()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 50m,
            Stock = 10,
            IsActive = false
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new AddToBasketCommand
        {
            ProductId = product.Id,
            Quantity = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found or is inactive");
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddToBasketCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
