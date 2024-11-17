using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductType : Entity<Guid>
{
    public ProductType(string name, Guid id) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product type name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Product type name cannot be longer than 100 characters", nameof(name));

        Name = name.Trim();
    }

    public string Name { get; }

    public override bool Equals(object? obj)
    {
        return obj is ProductType other && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return Name;
    }
}