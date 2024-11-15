namespace EasyBuy.Domain.ValueObjects;

public class ProductName
{
    public string Name { get; private set; }

    public ProductName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.");

        Name = name;
    }

    public override string ToString() => Name;
}