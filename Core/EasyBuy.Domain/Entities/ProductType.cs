using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductType : BaseEntity<int>
{
    public ProductType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        if (name.Length > 100) 
            throw new ArgumentException("Name is too long. Max length is 100 characters.", nameof(name));

        Name = name;
    }

    public string Name { get; }
    
    public override bool Equals(object? obj)
    {
        return obj is ProductType other && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}