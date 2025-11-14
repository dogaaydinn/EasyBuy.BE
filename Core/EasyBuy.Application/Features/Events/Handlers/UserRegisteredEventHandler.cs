using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Infrastructure;
using EasyBuy.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Events.Handlers;

/// <summary>
/// Handles UserRegisteredEvent by sending welcome email to new users.
/// Part of the event-driven architecture for user onboarding.
/// </summary>
public sealed class UserRegisteredEventHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IEmailService emailService,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling UserRegisteredEvent for user: {UserId}, Email: {Email}",
            domainEvent.UserId,
            domainEvent.Email);

        try
        {
            // Send welcome email
            await _emailService.SendEmailAsync(
                to: domainEvent.Email,
                subject: $"Welcome to EasyBuy, {domainEvent.UserName}!",
                body: GenerateWelcomeEmailBody(domainEvent.UserName, domainEvent.Email),
                isHtml: true,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Welcome email sent successfully to user: {UserId}",
                domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email to user: {UserId}, Email: {Email}",
                domainEvent.UserId,
                domainEvent.Email);

            // Don't throw - email failure shouldn't break registration
            // Consider implementing retry logic or dead letter queue
        }
    }

    private static string GenerateWelcomeEmailBody(string userName, string email)
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
                    .button {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to EasyBuy!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hi {userName},</h2>
                        <p>Thank you for registering with EasyBuy! We're excited to have you as part of our community.</p>
                        <p>Your account has been successfully created with the email: <strong>{email}</strong></p>
                        <p>Here's what you can do next:</p>
                        <ul>
                            <li>Browse our wide selection of products</li>
                            <li>Add items to your wishlist</li>
                            <li>Enjoy exclusive member discounts</li>
                            <li>Track your orders in real-time</li>
                        </ul>
                        <p style='text-align: center; margin-top: 30px;'>
                            <a href='https://easybuy.com/products' class='button'>Start Shopping</a>
                        </p>
                        <p style='margin-top: 30px; font-size: 12px; color: #666;'>
                            If you didn't create this account, please contact our support team immediately.
                        </p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
