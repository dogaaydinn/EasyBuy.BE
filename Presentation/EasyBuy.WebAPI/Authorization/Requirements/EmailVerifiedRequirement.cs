using Microsoft.AspNetCore.Authorization;

namespace EasyBuy.WebAPI.Authorization.Requirements;

/// <summary>
/// Authorization requirement that checks if user's email is verified.
/// </summary>
public class EmailVerifiedRequirement : IAuthorizationRequirement
{
    public EmailVerifiedRequirement()
    {
    }
}

/// <summary>
/// Handler for email verification requirement.
/// Checks the EmailConfirmed claim from Identity.
/// </summary>
public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
{
    private readonly ILogger<EmailVerifiedHandler> _logger;

    public EmailVerifiedHandler(ILogger<EmailVerifiedHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement)
    {
        // Check if user has email confirmed claim
        var emailConfirmedClaim = context.User.FindFirst("EmailConfirmed");

        if (emailConfirmedClaim != null && bool.TryParse(emailConfirmedClaim.Value, out var isConfirmed) && isConfirmed)
        {
            _logger.LogDebug("Email verified for user: {UserId}", context.User.Identity?.Name);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Email not verified for user: {UserId}", context.User.Identity?.Name);
            // Don't call context.Fail() - let other handlers run
        }

        return Task.CompletedTask;
    }
}
