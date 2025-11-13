using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Features.Auth.Commands.Register;
using EasyBuy.Application.Mappings;
using EasyBuy.Application.Tests.Helpers;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Tests.Features.Auth.Commands;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly EasyBuyDbContext _context;
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<RegisterCommandHandler>> _mockLogger;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockUserManager = TestDbContextFactory.GetMockUserManager();
        _mockTokenService = new Mock<ITokenService>();
        _mockPublisher = new Mock<IPublisher>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _handler = new RegisterCommandHandler(
            _mockUserManager.Object,
            _mockTokenService.Object,
            _mockPublisher.Object,
            _mockEmailService.Object,
            _context);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<string> { "Customer" });

        _mockTokenService.Setup(x => x.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IList<string>>()))
            .Returns("mock-access-token");

        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("mock-access-token");
        result.Data.RefreshToken.Should().Be("mock-refresh-token");
        result.Data.User.Should().NotBeNull();
        result.Data.User.Email.Should().Be(command.Email);
        result.Data.User.UserName.Should().Be(command.UserName);
        result.Data.User.Roles.Should().Contain("Customer");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new AppUser { Email = "test@example.com" };

        var command = new RegisterCommand
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email already exists");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new AppUser { UserName = "testuser" };

        var command = new RegisterCommand
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("username already exists");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var errors = new[]
        {
            new IdentityError { Description = "Password too weak" }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldStoreRefreshTokenInDatabase()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<string> { "Customer" });

        _mockTokenService.Setup(x => x.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IList<string>>()))
            .Returns("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwianRpIjoiMTIzNDU2Nzg5MCIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var refreshTokens = _context.RefreshTokens.ToList();
        refreshTokens.Should().HaveCount(1);
        refreshTokens[0].Token.Should().Be("mock-refresh-token");
        refreshTokens[0].IsUsed.Should().BeFalse();
        refreshTokens[0].IsRevoked.Should().BeFalse();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
