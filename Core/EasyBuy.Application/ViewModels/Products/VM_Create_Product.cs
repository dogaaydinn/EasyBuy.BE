namespace EasyBuy.Application.ViewModels.Products;

public class VmCreateProduct
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ICollection<Domain.Entities.Order> Orders { get; set; }
}