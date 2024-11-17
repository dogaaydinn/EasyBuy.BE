namespace EasyBuy.Domain.ValueObjects;

public class ProductName
{
    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name cannot be empty or null.", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}