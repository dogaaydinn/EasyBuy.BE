using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class BasketItem : BaseEntity
{
    public Guid BasketId { get; }
    public Guid ProductId { get; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public Basket Basket { get; set; }
    public Product Product { get; set; }

    public override string ToString()
    {
        return $"Basket ID: {BasketId}, Product ID: {ProductId}, Quantity: {Quantity}, Price: {Price}";
    }
}