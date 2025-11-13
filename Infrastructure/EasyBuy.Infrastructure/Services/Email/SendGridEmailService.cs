using EasyBuy.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EasyBuy.Infrastructure.Services.Email;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["EmailSettings:SendGridApiKey"] ?? throw new ArgumentNullException("SendGridApiKey not configured");
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@easybuy.com";
        _fromName = _configuration["EmailSettings:FromName"] ?? "EasyBuy";
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(new List<string> { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var recipients = to.Select(email => new EmailAddress(email)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                from,
                recipients,
                subject,
                isHtml ? null : body,
                isHtml ? body : null
            );

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", to));
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to EasyBuy!";
        var body = $@"
            <html>
            <body>
                <h2>Welcome {userName}!</h2>
                <p>Thank you for registering with EasyBuy. We're excited to have you on board.</p>
                <p>Start shopping for the best products at the best prices!</p>
                <br/>
                <p>Best regards,<br/>The EasyBuy Team</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendOrderConfirmationEmailAsync(string to, string orderNumber, decimal total, CancellationToken cancellationToken = default)
    {
        var subject = $"Order Confirmation - {orderNumber}";
        var body = $@"
            <html>
            <body>
                <h2>Order Confirmed!</h2>
                <p>Thank you for your order.</p>
                <p><strong>Order Number:</strong> {orderNumber}</p>
                <p><strong>Total Amount:</strong> ${total:F2}</p>
                <p>We'll send you another email when your order ships.</p>
                <br/>
                <p>Best regards,<br/>The EasyBuy Team</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Password Reset Request";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                <br/>
                <p>Best regards,<br/>The EasyBuy Team</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
