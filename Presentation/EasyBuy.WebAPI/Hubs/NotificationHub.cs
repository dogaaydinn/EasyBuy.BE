using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EasyBuy.WebAPI.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications.
/// Clients can connect to receive live updates about orders, products, and system events.
/// Requires authentication for all connections.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.Identity?.Name;

        _logger.LogInformation("Client connected to NotificationHub. ConnectionId: {ConnectionId}, User: {UserId}",
            connectionId, userId ?? "Anonymous");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.Identity?.Name;

        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with error. ConnectionId: {ConnectionId}, User: {UserId}",
                connectionId, userId ?? "Anonymous");
        }
        else
        {
            _logger.LogInformation("Client disconnected. ConnectionId: {ConnectionId}, User: {UserId}",
                connectionId, userId ?? "Anonymous");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific group (e.g., for order updates).
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group: {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave a specific group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group: {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Send a notification to all connected clients.
    /// This is a server-side method typically called from backend services.
    /// </summary>
    public async Task SendNotificationToAll(string message, string type = "info")
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Broadcast notification sent: {Message}", message);
    }

    /// <summary>
    /// Send a notification to a specific user.
    /// </summary>
    public async Task SendNotificationToUser(string userId, string message, string type = "info")
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Notification sent to user {UserId}: {Message}", userId, message);
    }

    /// <summary>
    /// Send a notification to a specific group.
    /// </summary>
    public async Task SendNotificationToGroup(string groupName, string message, string type = "info")
    {
        await Clients.Group(groupName).SendAsync("ReceiveNotification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Notification sent to group {GroupName}: {Message}", groupName, message);
    }
}
