using EasyBuy.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EasyBuy.Infrastructure.Services.Sms;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly bool _isEnabled;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _isEnabled = _configuration.GetValue<bool>("SmsSettings:EnableSms");

        if (_isEnabled)
        {
            var accountSid = _configuration["SmsSettings:TwilioAccountSid"];
            var authToken = _configuration["SmsSettings:TwilioAuthToken"];

            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                TwilioClient.Init(accountSid, authToken);
            }
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("SMS is disabled. Message not sent to {PhoneNumber}", phoneNumber);
            return;
        }

        try
        {
            var fromNumber = _configuration["SmsSettings:TwilioPhoneNumber"];
            if (string.IsNullOrEmpty(fromNumber))
            {
                _logger.LogError("Twilio phone number not configured");
                return;
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}", phoneNumber, messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task SendOrderStatusUpdateAsync(string phoneNumber, string orderNumber, string status, CancellationToken cancellationToken = default)
    {
        var message = $"Your order {orderNumber} status has been updated to: {status}. Thank you for shopping with EasyBuy!";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendVerificationCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var message = $"Your EasyBuy verification code is: {code}. This code will expire in 10 minutes.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}
