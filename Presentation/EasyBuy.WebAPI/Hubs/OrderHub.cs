using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EasyBuy.WebAPI.Hubs;

/// <summary>
/// SignalR Hub for real-time order updates.
/// Clients can subscribe to specific orders to receive live status updates.
/// Requires authentication for all connections.
/// </summary>
[Authorize]
public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
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

        _logger.LogInformation("Client connected to OrderHub. ConnectionId: {ConnectionId}, User: {UserId}",
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
    /// Subscribe to updates for a specific order.
    /// </summary>
    public async Task SubscribeToOrder(string orderId)
    {
        var groupName = $"Order_{orderId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Client {ConnectionId} subscribed to order: {OrderId}",
            Context.ConnectionId, orderId);

        await Clients.Caller.SendAsync("SubscribedToOrder", new
        {
            orderId,
            message = $"Successfully subscribed to order {orderId}",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Unsubscribe from updates for a specific order.
    /// </summary>
    public async Task UnsubscribeFromOrder(string orderId)
    {
        var groupName = $"Order_{orderId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Client {ConnectionId} unsubscribed from order: {OrderId}",
            Context.ConnectionId, orderId);

        await Clients.Caller.SendAsync("UnsubscribedFromOrder", new
        {
            orderId,
            message = $"Successfully unsubscribed from order {orderId}",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send order status update to all subscribers of an order.
    /// This is typically called from backend services when order status changes.
    /// </summary>
    public async Task SendOrderUpdate(string orderId, string status, string message)
    {
        var groupName = $"Order_{orderId}";

        await Clients.Group(groupName).SendAsync("OrderStatusChanged", new
        {
            orderId,
            status,
            message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Order update sent for order {OrderId}: {Status} - {Message}",
            orderId, status, message);
    }

    /// <summary>
    /// Send order payment confirmation to subscribers.
    /// </summary>
    public async Task SendPaymentConfirmation(string orderId, decimal amount, string paymentMethod)
    {
        var groupName = $"Order_{orderId}";

        await Clients.Group(groupName).SendAsync("PaymentConfirmed", new
        {
            orderId,
            amount,
            paymentMethod,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Payment confirmation sent for order {OrderId}: {Amount} via {PaymentMethod}",
            orderId, amount, paymentMethod);
    }

    /// <summary>
    /// Send shipment tracking update to subscribers.
    /// </summary>
    public async Task SendShipmentUpdate(string orderId, string trackingNumber, string carrier, string status)
    {
        var groupName = $"Order_{orderId}";

        await Clients.Group(groupName).SendAsync("ShipmentUpdated", new
        {
            orderId,
            trackingNumber,
            carrier,
            status,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Shipment update sent for order {OrderId}: {Carrier} {TrackingNumber} - {Status}",
            orderId, carrier, trackingNumber, status);
    }
}
