namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Exception thrown when user is not authenticated
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "You are not authorized to access this resource.")
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}
