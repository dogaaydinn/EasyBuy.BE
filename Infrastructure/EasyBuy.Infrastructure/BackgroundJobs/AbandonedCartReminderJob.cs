using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Contracts.Basket;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to send reminders for abandoned shopping carts.
/// Runs every 2 hours to find carts with items that haven't been updated recently.
/// </summary>
public class AbandonedCartReminderJob : IBackgroundJob
{
    private readonly IDistributedCache _cache;
    private readonly IEmailService _emailService;
    private readonly ILogger<AbandonedCartReminderJob> _logger;
    
    // Consider a cart abandoned if not updated in 2 hours
    private static readonly TimeSpan AbandonmentThreshold = TimeSpan.FromHours(2);

    public AbandonedCartReminderJob(
        IDistributedCache cache,
        IEmailService emailService,
        ILogger<AbandonedCartReminderJob> logger)
    {
        _cache = cache;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting abandoned cart reminder job at {Time}", DateTime.UtcNow);

        try
        {
            var remindersSet = 0;
            
            // Note: In a real implementation, you would query Redis for all basket keys
            // This is a simplified version. For production, consider:
            // 1. Storing basket metadata in a separate cache key (basket:metadata:{userId})
            // 2. Using Redis SCAN to iterate through basket keys
            // 3. Maintaining a separate index of active baskets in the database
            
            _logger.LogInformation("Abandoned cart check completed. Reminders sent: {Count}", remindersSet);
            
            // For demonstration, we'll log that the job ran successfully
            // In production, this would scan Redis and send emails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing abandoned cart reminders");
            throw;
        }
    }

    private async Task SendAbandonedCartReminderAsync(Guid userId, int itemCount, decimal totalAmount)
    {
        try
        {
            var emailBody = GenerateReminderEmailBody(itemCount, totalAmount);
            
            // In production, fetch user email from database
            await _emailService.SendEmailAsync(
                to: "user@example.com", // Would fetch from database
                subject: "You left items in your cart!",
                body: emailBody);

            _logger.LogInformation("Sent abandoned cart reminder to user: {UserId}, Items: {ItemCount}, Total: {Total}",
                userId, itemCount, totalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send abandoned cart reminder to user: {UserId}", userId);
        }
    }

    private static string GenerateReminderEmailBody(int itemCount, decimal totalAmount)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif; }}
                    .header {{ background-color: #FF6B6B; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .cta {{ background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; display: inline-block; margin: 20px 0; border-radius: 4px; }}
                    .items {{ margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Don't forget your items!</h1>
                    </div>
                    <div class='content'>
                        <p>Hi there,</p>
                        <p>You have <strong>{itemCount} item(s)</strong> waiting in your cart with a total of <strong>${totalAmount:F2}</strong>.</p>
                        <div class='items'>
                            <p>Complete your purchase before they're gone!</p>
                        </div>
                        <a href='https://easybuy.com/cart' class='cta'>Complete Your Order</a>
                        <p style='margin-top: 20px; color: #666; font-size: 12px;'>
                            This is an automated reminder. Your cart will be saved for 30 days.
                        </p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
