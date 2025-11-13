namespace EasyBuy.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendOrderStatusUpdateAsync(string phoneNumber, string orderNumber, string status, CancellationToken cancellationToken = default);
    Task SendVerificationCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
