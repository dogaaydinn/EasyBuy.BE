namespace EasyBuy.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.", "NOT_FOUND", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND", 404)
    {
    }
}
