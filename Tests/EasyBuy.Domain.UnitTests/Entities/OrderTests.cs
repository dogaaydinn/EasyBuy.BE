using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EasyBuy.Domain.UnitTests.Entities;

public class OrderTests
{
    [Fact]
    public void CalculateTotal_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var order = CreateTestOrder();
        var item1 = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            Product = null!,
            Quantity = 2,
            UnitPrice = 10.00m,
            ProductName = "Product 1"
        };
        var item2 = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            Product = null!,
            Quantity = 3,
            UnitPrice = 15.00m,
            ProductName = "Product 2"
        };

        order.OrderItems = new List<OrderItem> { item1, item2 };
        order.TaxAmount = 5.00m;
        order.ShippingCost = 10.00m;
        order.DiscountAmount = 0m;

        // Act
        order.CalculateTotal();

        // Assert
        order.Subtotal.Should().Be(65.00m); // (2*10 + 3*15)
        order.Total.Should().Be(80.00m); // 65 + 5 + 10 - 0
    }

    [Fact]
    public void CalculateTotal_WithDiscount_ShouldSubtractFromTotal()
    {
        // Arrange
        var order = CreateTestOrder();
        var item = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            Product = null!,
            Quantity = 1,
            UnitPrice = 100.00m,
            ProductName = "Product"
        };

        order.OrderItems = new List<OrderItem> { item };
        order.TaxAmount = 10.00m;
        order.ShippingCost = 5.00m;
        order.DiscountAmount = 15.00m;

        // Act
        order.CalculateTotal();

        // Assert
        order.Subtotal.Should().Be(100.00m);
        order.Total.Should().Be(100.00m); // 100 + 10 + 5 - 15
    }

    [Fact]
    public void MarkAsShipped_WithProcessingOrder_ShouldUpdateStatusAndRaiseEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        order.OrderStatus = OrderStatus.Processing;
        var trackingNumber = "TRACK123456";

        // Act
        order.MarkAsShipped(trackingNumber);

        // Assert
        order.OrderStatus.Should().Be(OrderStatus.Shipped);
        order.ShippedDate.Should().NotBeNull();
        order.ShippedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.TrackingNumber.Should().Be(trackingNumber);

        // Check domain event was raised
        order.DomainEvents.Should().ContainSingle();
        var domainEvent = order.DomainEvents.First();
        domainEvent.Should().BeOfType<OrderStatusChangedEvent>();

        var statusEvent = (OrderStatusChangedEvent)domainEvent;
        statusEvent.OrderId.Should().Be(order.Id);
        statusEvent.NewStatus.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void MarkAsShipped_WithNonProcessingOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.OrderStatus = OrderStatus.Pending;

        // Act
        Action act = () => order.MarkAsShipped("TRACK123");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only processing orders can be marked as shipped");
    }

    [Fact]
    public void Cancel_WithPendingOrder_ShouldCancelAndRaiseEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        order.OrderStatus = OrderStatus.Pending;
        var reason = "Customer requested cancellation";

        // Act
        order.Cancel(reason);

        // Assert
        order.OrderStatus.Should().Be(OrderStatus.Cancelled);
        order.CancelledDate.Should().NotBeNull();
        order.CancelledDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.Notes.Should().Contain(reason);

        // Check domain event
        order.DomainEvents.Should().ContainSingle();
        var domainEvent = order.DomainEvents.First();
        domainEvent.Should().BeOfType<OrderStatusChangedEvent>();

        var statusEvent = (OrderStatusChangedEvent)domainEvent;
        statusEvent.NewStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WithDeliveredOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.OrderStatus = OrderStatus.Delivered;

        // Act
        Action act = () => order.Cancel("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel order with status Delivered");
    }

    [Fact]
    public void Cancel_WithAlreadyCancelledOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.OrderStatus = OrderStatus.Cancelled;

        // Act
        Action act = () => order.Cancel("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel order with status Cancelled");
    }

    [Fact]
    public void OrderItem_TotalPrice_ShouldBeCalculatedCorrectly()
    {
        // Arrange & Act
        var item = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            Product = null!,
            Quantity = 5,
            UnitPrice = 12.50m,
            ProductName = "Test Product"
        };

        // Assert
        item.TotalPrice.Should().Be(62.50m);
    }

    private static Order CreateTestOrder()
    {
        return new Order
        {
            OrderNumber = "TEST-001",
            OrderStatus = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            AppUserId = "user-123",
            AppUser = new AppUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            },
            DeliveryId = Guid.NewGuid(),
            Delivery = new Delivery
            {
                DeliveryMethod = "Standard",
                ShortName = "STD",
                DeliveryTime = "3-5 days",
                Description = "Standard delivery",
                Price = 5.00m
            },
            OrderItems = new List<OrderItem>()
        };
    }
}
