namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Exception thrown when user doesn't have permission to perform an action
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You don't have permission to perform this action.")
        : base(message, "FORBIDDEN", 403)
    {
    }
}
