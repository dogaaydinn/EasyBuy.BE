using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductType : BaseEntity<int>
{
    public ProductType(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Name = name;
    }
    public string Name { get; }
    public override string ToString()
    {
        return Name;
    }
}