using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Features.Auth.Commands.Login;
using EasyBuy.Application.Tests.Helpers;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;

namespace EasyBuy.Application.Tests.Features.Auth.Commands;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly EasyBuyDbContext _context;
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockUserManager = TestDbContextFactory.GetMockUserManager();
        _mockSignInManager = TestDbContextFactory.GetMockSignInManager(_mockUserManager);
        _mockTokenService = new Mock<ITokenService>();

        _handler = new LoginCommandHandler(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockTokenService.Object,
            _context);
    }

    [Fact]
    public async Task Handle_ValidEmailAndPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User"
        };

        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
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
        result.Data.Should().NotBeNull();
        result.Data!.User.Email.Should().Be(user.Email);
        result.Data.User.Roles.Should().Contain("Customer");
    }

    [Fact]
    public async Task Handle_ValidUsernameAndPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser"
        };

        var command = new LoginCommand
        {
            EmailOrUsername = "testuser",
            Password = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(command.EmailOrUsername))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
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
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser
        {
            Email = "test@example.com",
            UserName = "testuser"
        };

        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "WrongPassword"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email/username or password");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            EmailOrUsername = "nonexistent@example.com",
            Password = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync((AppUser?)null);

        _mockUserManager.Setup(x => x.FindByNameAsync(command.EmailOrUsername))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email/username or password");
    }

    [Fact]
    public async Task Handle_DeletedUser_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser
        {
            Email = "test@example.com",
            UserName = "testuser",
            IsDeleted = true
        };

        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("deactivated");
    }

    [Fact]
    public async Task Handle_LockedOutUser_ShouldReturnFailure()
    {
        // Arrange
        var user = new AppUser
        {
            Email = "test@example.com",
            UserName = "testuser"
        };

        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "Test@123456"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.EmailOrUsername))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, false))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("locked");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
