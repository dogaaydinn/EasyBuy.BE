using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Infrastructure;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles OrderStatusChangedEvent by notifying users of order status updates via SMS.
/// Provides real-time order tracking notifications.
/// </summary>
public sealed class OrderStatusChangedEventHandler : IDomainEventHandler<OrderStatusChangedEvent>
{
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        ISmsService smsService,
        IEmailService emailService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling OrderStatusChangedEvent for Order: {OrderId}, Status changed from {OldStatus} to {NewStatus}",
            domainEvent.OrderId,
            domainEvent.OldStatus,
            domainEvent.NewStatus);

        try
        {
            // Send SMS notification
            if (!string.IsNullOrEmpty(domainEvent.UserPhoneNumber))
            {
                await SendStatusChangeSms(
                    domainEvent.UserPhoneNumber,
                    domainEvent.OrderId,
                    domainEvent.NewStatus,
                    cancellationToken);
            }

            // Send email notification for important status changes
            if (IsImportantStatusChange(domainEvent.NewStatus))
            {
                await SendStatusChangeEmail(
                    domainEvent.UserEmail,
                    domainEvent.OrderId,
                    domainEvent.NewStatus,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Order status change notification sent successfully for Order: {OrderId}",
                domainEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send order status change notification for Order: {OrderId}",
                domainEvent.OrderId);
        }
    }

    private async Task SendStatusChangeSms(
        string phoneNumber,
        Guid orderId,
        string newStatus,
        CancellationToken cancellationToken)
    {
        var message = newStatus.ToLower() switch
        {
            "shipped" => $"EasyBuy: Your order #{orderId.ToString()[..8]} has been shipped! Track it at easybuy.com/orders",
            "delivered" => $"EasyBuy: Your order #{orderId.ToString()[..8]} has been delivered. Enjoy your purchase!",
            "cancelled" => $"EasyBuy: Your order #{orderId.ToString()[..8]} has been cancelled. Refund will be processed within 3-5 business days.",
            _ => $"EasyBuy: Your order #{orderId.ToString()[..8]} status updated to: {newStatus}. Check easybuy.com/orders"
        };

        await _smsService.SendSmsAsync(
            to: phoneNumber,
            message: message,
            cancellationToken: cancellationToken);
    }

    private async Task SendStatusChangeEmail(
        string email,
        Guid orderId,
        string newStatus,
        CancellationToken cancellationToken)
    {
        var subject = $"Order Status Update - #{orderId.ToString()[..8]}";
        var body = GenerateStatusChangeEmailBody(orderId, newStatus);

        await _emailService.SendEmailAsync(
            to: email,
            subject: subject,
            body: body,
            isHtml: true,
            cancellationToken: cancellationToken);
    }

    private static bool IsImportantStatusChange(string status)
    {
        var importantStatuses = new[] { "shipped", "delivered", "cancelled" };
        return importantStatuses.Contains(status.ToLower());
    }

    private static string GenerateStatusChangeEmailBody(Guid orderId, string newStatus)
    {
        var statusMessage = newStatus.ToLower() switch
        {
            "shipped" => "Your order is on its way!",
            "delivered" => "Your order has been delivered!",
            "cancelled" => "Your order has been cancelled.",
            _ => $"Your order status has been updated to: {newStatus}"
        };

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .status {{ font-size: 24px; color: #FF9800; font-weight: bold; }}
                    .button {{ display: inline-block; padding: 10px 20px; background-color: #FF9800; color: white; text-decoration: none; border-radius: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Order Status Update</h1>
                    </div>
                    <div class='content'>
                        <p class='status'>{statusMessage}</p>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        <p><strong>New Status:</strong> {newStatus}</p>
                        <p style='text-align: center; margin-top: 30px;'>
                            <a href='https://easybuy.com/orders/{orderId}' class='button'>View Order Details</a>
                        </p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
