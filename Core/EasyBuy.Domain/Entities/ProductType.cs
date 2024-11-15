using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductType : BaseEntity<int>
{
    public string Name { get; set; }
}
