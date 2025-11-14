namespace EasyBuy.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time notifications via SignalR.
/// Use this service from application layer to send notifications to connected clients.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>
    /// Send a notification to all connected clients.
    /// </summary>
    Task SendNotificationToAllAsync(string message, string type = "info", CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a notification to a specific user.
    /// </summary>
    Task SendNotificationToUserAsync(string userId, string message, string type = "info", CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a notification to a specific group.
    /// </summary>
    Task SendNotificationToGroupAsync(string groupName, string message, string type = "info", CancellationToken cancellationToken = default);

    /// <summary>
    /// Send order status update to all subscribers of an order.
    /// </summary>
    Task SendOrderUpdateAsync(string orderId, string status, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send payment confirmation for an order.
    /// </summary>
    Task SendPaymentConfirmationAsync(string orderId, decimal amount, string paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send shipment tracking update.
    /// </summary>
    Task SendShipmentUpdateAsync(string orderId, string trackingNumber, string carrier, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send product stock update notification.
    /// </summary>
    Task SendProductStockUpdateAsync(string productId, int currentStock, string message, CancellationToken cancellationToken = default);
}
