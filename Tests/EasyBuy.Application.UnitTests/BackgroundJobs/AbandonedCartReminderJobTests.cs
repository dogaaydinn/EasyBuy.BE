using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Domain.Entities;
using EasyBuy.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.UnitTests.BackgroundJobs;

public class AbandonedCartReminderJobTests
{
    private readonly Mock<IBasketService> _basketServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<AbandonedCartReminderJob>> _loggerMock;
    private readonly AbandonedCartReminderJob _job;

    public AbandonedCartReminderJobTests()
    {
        _basketServiceMock = new Mock<IBasketService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<AbandonedCartReminderJob>>();

        _job = new AbandonedCartReminderJob(
            _basketServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_NoAbandonedCarts_ShouldNotSendEmails()
    {
        // Arrange
        var emptyBaskets = new List<BasketDto>();
        _basketServiceMock
            .Setup(x => x.GetAbandonedCartsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyBaskets);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithAbandonedCarts_ShouldSendReminderEmails()
    {
        // Arrange
        var abandonedCarts = new List<BasketDto>
        {
            new BasketDto
            {
                Id = "cart1",
                UserId = Guid.NewGuid(),
                UserEmail = "user1@example.com",
                Items = new List<BasketItemDto>
                {
                    new BasketItemDto
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 1",
                        Quantity = 2,
                        Price = 29.99m
                    }
                },
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new BasketDto
            {
                Id = "cart2",
                UserId = Guid.NewGuid(),
                UserEmail = "user2@example.com",
                Items = new List<BasketItemDto>
                {
                    new BasketItemDto
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 2",
                        Quantity = 1,
                        Price = 49.99m
                    }
                },
                UpdatedAt = DateTime.UtcNow.AddHours(-5)
            }
        };

        _basketServiceMock
            .Setup(x => x.GetAbandonedCartsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(abandonedCarts);

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.Is<string>(email => email == "user1@example.com" || email == "user2@example.com"),
                It.Is<string>(subject => subject.Contains("Don't forget") || subject.Contains("cart")),
                It.IsAny<string>(),
                true),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_EmailSendFails_ShouldContinueWithOtherCarts()
    {
        // Arrange
        var abandonedCarts = new List<BasketDto>
        {
            new BasketDto
            {
                Id = "cart1",
                UserId = Guid.NewGuid(),
                UserEmail = "user1@example.com",
                Items = new List<BasketItemDto> { new() { ProductName = "Product 1", Quantity = 1, Price = 10 } },
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new BasketDto
            {
                Id = "cart2",
                UserId = Guid.NewGuid(),
                UserEmail = "user2@example.com",
                Items = new List<BasketItemDto> { new() { ProductName = "Product 2", Quantity = 1, Price = 20 } },
                UpdatedAt = DateTime.UtcNow.AddHours(-4)
            }
        };

        _basketServiceMock
            .Setup(x => x.GetAbandonedCartsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(abandonedCarts);

        // First email fails, second succeeds
        _emailServiceMock
            .SetupSequence(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                true),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ShouldStopProcessing()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - should not throw
        await _job.ExecuteAsync(cts.Token);
    }
}
