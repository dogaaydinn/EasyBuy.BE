using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Product : BaseEntity<int>
{
    public Product(string name, string description, decimal price, int productTypeId,
        int productBrandId, ProductType productType, ProductBrand productBrand, List<Order> orders)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) 
            throw new ArgumentException("Product description cannot be empty", nameof(description));
        if (price < 0) 
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Price = price;
        ProductTypeId = productTypeId;
        ProductBrandId = productBrandId;
        ProductType = productType ?? throw new ArgumentNullException(nameof(productType), "Product type cannot be null.");
        ProductBrand = productBrand ?? throw new ArgumentNullException(nameof(productBrand), "Product brand cannot be null.");
        _orders = orders;
    }

    public string Name { get; }
    public string Description { get; }
    public decimal Price { get; }
    public ProductType ProductType { get; private set; }
    public int ProductTypeId { get; private set; }
    public ProductBrand ProductBrand { get; private set; }
    public int ProductBrandId { get; private set; }
    private readonly List<Order> _orders;
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
        
    public decimal GetPriceAfterDiscount(decimal discountPercentage)
    {
        if (discountPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage),
                "Discount percentage must be between 0 and 100.");

        return Price - (Price * discountPercentage / 100);
    }
        
    public void AddOrder(Order order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        _orders.Add(order);
    }

    public void RemoveOrder(Order order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        _orders.Remove(order);
    }
        
    public override string ToString()
    {
        return $"{Name} - {Description} - {Price:C} - Type: {ProductType.Name} - Brand: {ProductBrand.Name}";
    }
}