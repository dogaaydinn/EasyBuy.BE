using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class BasketItem : BaseEntity
{
    public required Product Product { get; set; } //FK
    [Range(1, int.MaxValue)]
    public required int Quantity { get; set; }
    public required Basket Basket { get; set; }
}