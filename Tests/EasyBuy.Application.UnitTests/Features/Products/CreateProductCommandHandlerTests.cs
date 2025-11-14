using AutoFixture;
using AutoFixture.Xunit2;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Products.Commands.CreateProduct;
using EasyBuy.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for CreateProductCommandHandler following AAA pattern (Arrange, Act, Assert).
/// Demonstrates enterprise-grade testing with mocking, auto-fixture, and behavior verification.
/// </summary>
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _mockRepository;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _mockLogger;
    private readonly IFixture _fixture;
    private readonly CreateProductCommandHandler _sut; // System Under Test

    public CreateProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IProductWriteRepository>();
        _mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
        _fixture = new Fixture();

        // Customize fixture to create valid test data
        _fixture.Customize<CreateProductCommand>(c => c
            .With(x => x.Name, "Test Product")
            .With(x => x.Description, "Test Description")
            .With(x => x.Price, 99.99m)
            .With(x => x.Stock, 100));

        _sut = new CreateProductCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = _fixture.Create<CreateProductCommand>();
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = productId; return p; });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Product>(p =>
                p.Name == command.Name &&
                p.Description == command.Description &&
                p.Price == command.Price &&
                p.Stock == command.Stock)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnCreatedProductId()
    {
        // Arrange
        var command = _fixture.Create<CreateProductCommand>();
        var expectedId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = expectedId; return p; });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Data.Should().Be(expectedId.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_InvalidPrice_ShouldReturnFailure(decimal invalidPrice)
    {
        // Arrange
        var command = _fixture.Build<CreateProductCommand>()
            .With(x => x.Price, invalidPrice)
            .Create();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("price");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_InvalidName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var command = _fixture.Build<CreateProductCommand>()
            .With(x => x.Name, invalidName!)
            .Create();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("name");
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = _fixture.Create<CreateProductCommand>();
        var exceptionMessage = "Database connection failed";

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<CreateProductCommand>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _sut.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldLogInformation()
    {
        // Arrange
        var command = _fixture.Create<CreateProductCommand>();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = Guid.NewGuid(); return p; });

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory, AutoData]
    public async Task Handle_WithAutoData_ShouldCreateProduct(string name, string description, decimal price, int stock)
    {
        // This demonstrates using AutoFixture's [AutoData] attribute
        // Arrange
        var command = new CreateProductCommand
        {
            Name = name,
            Description = description,
            Price = Math.Abs(price), // Ensure positive
            Stock = Math.Abs(stock)  // Ensure positive
        };

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = Guid.NewGuid(); return p; });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
    }
}
