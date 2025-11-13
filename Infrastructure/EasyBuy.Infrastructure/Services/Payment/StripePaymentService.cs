using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace EasyBuy.Infrastructure.Services.Payment;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["PaymentSettings:Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe SecretKey not configured");
        _webhookSecret = _configuration["PaymentSettings:Stripe:WebhookSecret"] ?? "";

        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency.ToLower(),
                Description = request.Description,
                ReceiptEmail = request.CustomerEmail,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            // Add payment method if provided
            if (!string.IsNullOrEmpty(request.CardNumber))
            {
                // In production, you should use Stripe Elements on the frontend
                // and pass the payment method ID, not raw card details
                _logger.LogWarning("Raw card details should not be sent to backend. Use Stripe Elements.");
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment intent created: {PaymentIntentId}", paymentIntent.Id);

            return new PaymentResult
            {
                IsSuccess = paymentIntent.Status == "succeeded" || paymentIntent.Status == "processing",
                TransactionId = paymentIntent.Id,
                Status = MapStripeStatusToPaymentStatus(paymentIntent.Status),
                ProcessedAt = DateTime.UtcNow,
                Message = paymentIntent.Status == "succeeded" ? "Payment processed successfully" : $"Payment status: {paymentIntent.Status}"
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment failed: {Message}", ex.Message);

            return new PaymentResult
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorCode = ex.StripeError?.Code,
                Message = ex.StripeError?.Message ?? ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = (long)(amount * 100) // Convert to cents
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Refund created: {RefundId} for PaymentIntent: {PaymentIntentId}", refund.Id, transactionId);

            return new PaymentResult
            {
                IsSuccess = refund.Status == "succeeded",
                TransactionId = refund.Id,
                Status = refund.Status == "succeeded" ? PaymentStatus.Refunded : PaymentStatus.Failed,
                ProcessedAt = DateTime.UtcNow,
                Message = refund.Status == "succeeded" ? "Refund processed successfully" : $"Refund status: {refund.Status}"
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed: {Message}", ex.Message);

            return new PaymentResult
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorCode = ex.StripeError?.Code,
                Message = ex.StripeError?.Message ?? ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _webhookSecret
            );

            _logger.LogInformation("Webhook signature verified for event: {EventType}", stripeEvent.Type);
            return Task.FromResult(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Webhook signature verification failed");
            return Task.FromResult(false);
        }
    }

    private static PaymentStatus MapStripeStatusToPaymentStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "succeeded" => PaymentStatus.Completed,
            "processing" => PaymentStatus.Processing,
            "requires_payment_method" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            "requires_action" => PaymentStatus.Pending,
            "canceled" => PaymentStatus.Cancelled,
            _ => PaymentStatus.Failed
        };
    }
}
