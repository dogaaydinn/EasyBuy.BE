using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductBrand : BaseEntity<int>
{
    public string Name { get; set; }
}