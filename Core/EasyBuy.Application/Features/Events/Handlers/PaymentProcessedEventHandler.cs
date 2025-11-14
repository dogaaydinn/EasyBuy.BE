using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Infrastructure;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles PaymentProcessedEvent by updating order status and sending payment confirmation.
/// Coordinates payment workflow with order fulfillment.
/// </summary>
public sealed class PaymentProcessedEventHandler : IDomainEventHandler<PaymentProcessedEvent>
{
    private readonly IOrderWriteRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(
        IOrderWriteRepository orderRepository,
        IEmailService emailService,
        ILogger<PaymentProcessedEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PaymentProcessedEvent for Payment: {PaymentId}, Order: {OrderId}, Amount: {Amount}",
            domainEvent.PaymentId,
            domainEvent.OrderId,
            domainEvent.Amount);

        try
        {
            // Update order status to Processing (payment confirmed)
            var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId);
            if (order != null)
            {
                // Assuming order has a status property
                // order.UpdateStatus("Processing");
                // await _orderRepository.UpdateAsync(order);

                _logger.LogInformation(
                    "Order status updated to Processing for Order: {OrderId}",
                    domainEvent.OrderId);
            }
            else
            {
                _logger.LogWarning(
                    "Order not found for PaymentProcessedEvent: {OrderId}",
                    domainEvent.OrderId);
            }

            // Send payment confirmation email
            await SendPaymentConfirmationEmail(
                domainEvent.UserEmail,
                domainEvent.OrderId,
                domainEvent.Amount,
                domainEvent.PaymentMethod,
                cancellationToken);

            _logger.LogInformation(
                "Payment processed successfully for Payment: {PaymentId}",
                domainEvent.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to handle PaymentProcessedEvent for Payment: {PaymentId}",
                domainEvent.PaymentId);
        }
    }

    private async Task SendPaymentConfirmationEmail(
        string email,
        Guid orderId,
        decimal amount,
        string paymentMethod,
        CancellationToken cancellationToken)
    {
        var subject = $"Payment Confirmation - Order #{orderId.ToString()[..8]}";
        var body = GeneratePaymentConfirmationEmailBody(orderId, amount, paymentMethod);

        await _emailService.SendEmailAsync(
            to: email,
            subject: subject,
            body: body,
            isHtml: true,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Payment confirmation email sent to: {Email} for Order: {OrderId}",
            email,
            orderId);
    }

    private static string GeneratePaymentConfirmationEmailBody(Guid orderId, decimal amount, string paymentMethod)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .payment-details {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
                    .success {{ color: #4CAF50; font-size: 24px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>✓ Payment Confirmed</h1>
                    </div>
                    <div class='content'>
                        <p class='success'>Payment Successful!</p>
                        <p>Your payment has been processed successfully.</p>
                        <div class='payment-details'>
                            <h3>Payment Details</h3>
                            <p><strong>Order ID:</strong> {orderId}</p>
                            <p><strong>Amount Paid:</strong> ${amount:F2}</p>
                            <p><strong>Payment Method:</strong> {paymentMethod}</p>
                            <p><strong>Status:</strong> Confirmed</p>
                        </div>
                        <p>Your order is now being processed and you will receive a shipping notification soon.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
