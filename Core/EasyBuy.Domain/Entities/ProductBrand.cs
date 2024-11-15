using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductBrand : BaseEntity<int>
{
    public ProductBrand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Brand name cannot be longer than 100 characters", nameof(name));

        Name = name;
    }
    public string Name { get; }
    public override bool Equals(object? obj)
    {
        return obj is ProductBrand other && Name == other.Name;
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