using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductItemOrdered : BaseEntity
{
    public Guid ProductId { get; }
    [StringLength(127)]
    public string ProductName { get; set; }
    public Price ProductPrice { get; set; }
}