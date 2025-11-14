using EasyBuy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EasyBuy.WebAPI.Authorization.Requirements;

/// <summary>
/// Authorization requirement for managing reviews.
/// Users can only manage their own reviews unless they are Admin or Manager (moderation).
/// </summary>
public class ManageReviewRequirement : IAuthorizationRequirement
{
    public ManageReviewRequirement()
    {
    }
}

/// <summary>
/// Handler for review management authorization.
/// Checks if user owns the review or has Admin/Manager role for moderation.
/// </summary>
public class ManageReviewHandler : AuthorizationHandler<ManageReviewRequirement, Review>
{
    private readonly ILogger<ManageReviewHandler> _logger;

    public ManageReviewHandler(ILogger<ManageReviewHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageReviewRequirement requirement,
        Review resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return Task.CompletedTask;
        }

        // Admins and Managers can moderate all reviews
        if (context.User.IsInRole("Admin") || context.User.IsInRole("Manager"))
        {
            _logger.LogDebug("User {UserId} authorized to manage review {ReviewId} via Admin/Manager role",
                userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Users can only manage their own reviews
        if (resource.UserId.ToString() == userId)
        {
            _logger.LogDebug("User {UserId} authorized to manage their own review {ReviewId}",
                userId, resource.Id);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} not authorized to manage review {ReviewId} (owned by {OwnerId})",
                userId, resource.Id, resource.UserId);
        }

        return Task.CompletedTask;
    }
}
