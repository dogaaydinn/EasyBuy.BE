using EasyBuy.WebAPI.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EasyBuy.Application.UnitTests.Authorization;

public class EmailVerifiedHandlerTests
{
    private readonly Mock<ILogger<EmailVerifiedHandler>> _loggerMock;
    private readonly EmailVerifiedHandler _handler;

    public EmailVerifiedHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EmailVerifiedHandler>>();
        _handler = new EmailVerifiedHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_EmailConfirmed_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("EmailConfirmed", "true"),
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_EmailNotConfirmed_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("EmailConfirmed", "false"),
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoEmailConfirmedClaim_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_InvalidEmailConfirmedValue_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("EmailConfirmed", "invalid"),
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
