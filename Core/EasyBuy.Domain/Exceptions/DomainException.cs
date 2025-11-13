namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level exceptions
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected DomainException(string message, string code, int statusCode = 400)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    protected DomainException(string message, string code, Exception innerException, int statusCode = 400)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
