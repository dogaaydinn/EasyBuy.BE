namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Exception thrown when an external service fails
/// </summary>
public class ExternalServiceException : DomainException
{
    public ExternalServiceException(string serviceName, string message)
        : base($"External service '{serviceName}' failed: {message}", "EXTERNAL_SERVICE_ERROR", 502)
    {
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"External service '{serviceName}' failed: {message}", "EXTERNAL_SERVICE_ERROR", innerException, 502)
    {
    }
}
