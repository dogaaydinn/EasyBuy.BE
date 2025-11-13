using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Application.Features.Orders.Commands.CreateOrder;
using EasyBuy.Application.Mappings;
using EasyBuy.Application.Tests.Helpers;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Tests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests : IDisposable
{
    private readonly EasyBuyDbContext _context;
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly IMapper _mapper;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private readonly CreateOrderCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CreateOrderCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockUserManager = TestDbContextFactory.GetMockUserManager();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockPublisher = new Mock<IPublisher>();
        _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _handler = new CreateOrderCommandHandler(
            _context,
            _mockUserManager.Object,
            _mockCurrentUserService.Object,
            _mockPaymentService.Object,
            _mapper,
            _mockPublisher.Object,
            _mockLogger.Object);

        // Setup default mocks
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var user = new AppUser
        {
            Id = _testUserId,
            Email = "test@example.com",
            UserName = "testuser"
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            Stock = 10,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 2 }
            },
            PaymentType = PaymentType.CreditCard,
            PaymentDetails = "card-token"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        _mockPaymentService.Setup(x => x.ProcessPaymentAsync(
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OrderNumber.Should().StartWith("ORD-");
        result.Data.Total.Should().BeGreaterThan(0);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Status.Should().Be(OrderStatus.Confirmed);

        var order = _context.Orders.FirstOrDefault();
        order.Should().NotBeNull();
        order!.OrderItems.Should().HaveCount(1);
        order.Payments.Should().HaveCount(1);
        order.Payments.First().Status.Should().Be(PaymentStatus.Completed);

        // Verify stock was reduced
        var updatedProduct = _context.Products.Find(product.Id);
        updatedProduct!.Stock.Should().Be(8); // 10 - 2
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser { Id = _testUserId };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            Stock = 1,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 5 }
            },
            PaymentType = PaymentType.CashOnDelivery
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_InvalidProduct_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser { Id = _testUserId };

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Non-existent", Price = 100m, Quantity = 1 }
            },
            PaymentType = PaymentType.CashOnDelivery
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found or inactive");
    }

    [Fact]
    public async Task Handle_ValidCouponCode_ShouldApplyDiscount()
    {
        // Arrange
        var user = new AppUser { Id = _testUserId };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            Stock = 10,
            IsActive = true
        };

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "DISCOUNT10",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            MinimumOrderAmount = 50,
            UsageCount = 0
        };

        _context.Products.Add(product);
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 2 }
            },
            CouponCode = "DISCOUNT10",
            PaymentType = PaymentType.CashOnDelivery
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var order = _context.Orders.FirstOrDefault();
        order.Should().NotBeNull();
        order!.Discount.Should().Be(20m); // 10% of 200
        order.CouponId.Should().Be(coupon.Id);

        // Verify coupon usage count incremented
        var updatedCoupon = _context.Coupons.Find(coupon.Id);
        updatedCoupon!.UsageCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_PaymentFails_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser { Id = _testUserId };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            Stock = 10,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 1 }
            },
            PaymentType = PaymentType.CreditCard,
            PaymentDetails = "invalid-card"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        _mockPaymentService.Setup(x => x.ProcessPaymentAsync(
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Payment processing failed");
    }

    [Fact]
    public async Task Handle_CashOnDelivery_ShouldConfirmOrderImmediately()
    {
        // Arrange
        var user = new AppUser { Id = _testUserId };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            Stock = 10,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 1 }
            },
            PaymentType = PaymentType.CashOnDelivery
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var order = _context.Orders.FirstOrDefault();
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Confirmed);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
