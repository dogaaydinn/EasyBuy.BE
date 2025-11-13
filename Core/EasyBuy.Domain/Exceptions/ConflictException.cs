namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Exception thrown when there's a conflict (e.g., duplicate resource)
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", 409)
    {
    }
}
