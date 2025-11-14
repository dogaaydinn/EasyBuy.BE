using EasyBuy.Application.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for PaymentProcessedIntegrationEvent.
/// Handles payment confirmation and triggers order fulfillment workflow.
/// </summary>
public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedIntegrationEvent>
{
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PaymentProcessedIntegrationEvent: PaymentId={PaymentId}, OrderId={OrderId}, Amount={Amount}, Status={Status}",
            message.PaymentId,
            message.OrderId,
            message.Amount,
            message.Status);

        try
        {
            // 1. Update order status to "Payment Confirmed"
            _logger.LogInformation(
                "Updating order status to 'Payment Confirmed' for order {OrderId}",
                message.OrderId);

            // TODO: Update order status
            // await _orderService.UpdateStatusAsync(message.OrderId, "PaymentConfirmed");

            // 2. Trigger fulfillment workflow
            if (message.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Triggering fulfillment workflow for order {OrderId}",
                    message.OrderId);

                // TODO: Publish fulfillment event
                // await context.Publish(new FulfillmentRequestedEvent
                // {
                //     OrderId = message.OrderId,
                //     PaymentId = message.PaymentId
                // });
            }

            // 3. Record accounting entry
            _logger.LogInformation(
                "Recording accounting entry for payment {PaymentId}: {Amount} {Currency}",
                message.PaymentId,
                message.Amount,
                message.Currency);

            // TODO: Send to accounting service
            // await _accountingService.RecordPaymentAsync(message);

            // 4. Update analytics
            _logger.LogDebug(
                "Updating payment analytics: Method={PaymentMethod}, Amount={Amount}",
                message.PaymentMethod,
                message.Amount);

            // TODO: Track payment metrics
            // await _analyticsService.TrackPaymentAsync(message);

            _logger.LogInformation(
                "Successfully processed PaymentProcessedIntegrationEvent for payment {PaymentId}",
                message.PaymentId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PaymentProcessedIntegrationEvent for payment {PaymentId}. Message will be retried.",
                message.PaymentId);

            throw;
        }
    }
}
