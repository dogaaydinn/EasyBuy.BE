using EasyBuy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EasyBuy.WebAPI.Authorization.Requirements;

/// <summary>
/// Authorization requirement for managing orders.
/// Users can only manage their own orders unless they are Admin or Manager.
/// </summary>
public class ManageOrderRequirement : IAuthorizationRequirement
{
    public ManageOrderRequirement()
    {
    }
}

/// <summary>
/// Handler for order management authorization.
/// Checks if user owns the order or has Admin/Manager role.
/// </summary>
public class ManageOrderHandler : AuthorizationHandler<ManageOrderRequirement, Order>
{
    private readonly ILogger<ManageOrderHandler> _logger;

    public ManageOrderHandler(ILogger<ManageOrderHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageOrderRequirement requirement,
        Order resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return Task.CompletedTask;
        }

        // Admins and Managers can manage all orders
        if (context.User.IsInRole("Admin") || context.User.IsInRole("Manager"))
        {
            _logger.LogDebug("User {UserId} authorized to manage order {OrderId} via Admin/Manager role",
                userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Users can only manage their own orders
        if (resource.UserId.ToString() == userId)
        {
            _logger.LogDebug("User {UserId} authorized to manage their own order {OrderId}",
                userId, resource.Id);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} not authorized to manage order {OrderId} (owned by {OwnerId})",
                userId, resource.Id, resource.UserId);
        }

        return Task.CompletedTask;
    }
}
