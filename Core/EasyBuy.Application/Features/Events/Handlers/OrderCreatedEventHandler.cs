using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Infrastructure;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles OrderCreatedEvent by sending order confirmation email and SMS.
/// Implements order confirmation workflow as part of event-driven architecture.
/// </summary>
public sealed class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IOrderReadRepository _orderRepository;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IEmailService emailService,
        ISmsService smsService,
        IOrderReadRepository orderRepository,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling OrderCreatedEvent for Order: {OrderId}, User: {UserId}",
            domainEvent.OrderId,
            domainEvent.UserId);

        try
        {
            // Fetch order details
            var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning(
                    "Order not found for OrderCreatedEvent: {OrderId}",
                    domainEvent.OrderId);
                return;
            }

            // Send confirmation email
            await SendOrderConfirmationEmail(
                domainEvent.UserEmail,
                domainEvent.OrderId,
                domainEvent.TotalAmount,
                cancellationToken);

            // Send SMS notification if phone number is available
            if (!string.IsNullOrEmpty(domainEvent.UserPhoneNumber))
            {
                await SendOrderConfirmationSms(
                    domainEvent.UserPhoneNumber,
                    domainEvent.OrderId,
                    domainEvent.TotalAmount,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Order confirmation sent successfully for Order: {OrderId}",
                domainEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send order confirmation for Order: {OrderId}",
                domainEvent.OrderId);

            // Don't throw - notification failure shouldn't break order creation
        }
    }

    private async Task SendOrderConfirmationEmail(
        string email,
        Guid orderId,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        var subject = $"Order Confirmation - Order #{orderId.ToString()[..8]}";
        var body = GenerateOrderConfirmationEmailBody(orderId, totalAmount);

        await _emailService.SendEmailAsync(
            to: email,
            subject: subject,
            body: body,
            isHtml: true,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Order confirmation email sent to: {Email} for Order: {OrderId}",
            email,
            orderId);
    }

    private async Task SendOrderConfirmationSms(
        string phoneNumber,
        Guid orderId,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        var message = $"EasyBuy: Your order #{orderId.ToString()[..8]} of ${totalAmount:F2} has been confirmed. Track it at easybuy.com/orders";

        await _smsService.SendSmsAsync(
            to: phoneNumber,
            message: message,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Order confirmation SMS sent to: {PhoneNumber} for Order: {OrderId}",
            phoneNumber,
            orderId);
    }

    private static string GenerateOrderConfirmationEmailBody(Guid orderId, decimal totalAmount)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .order-details {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #2196F3; }}
                    .button {{ display: inline-block; padding: 10px 20px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Order Confirmed!</h1>
                    </div>
                    <div class='content'>
                        <p>Thank you for your order! We've received your order and it's being processed.</p>
                        <div class='order-details'>
                            <h3>Order Details</h3>
                            <p><strong>Order ID:</strong> {orderId}</p>
                            <p><strong>Total Amount:</strong> ${totalAmount:F2}</p>
                            <p><strong>Status:</strong> Processing</p>
                        </div>
                        <p>You will receive another email when your order ships.</p>
                        <p style='text-align: center; margin-top: 30px;'>
                            <a href='https://easybuy.com/orders/{orderId}' class='button'>Track Your Order</a>
                        </p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
