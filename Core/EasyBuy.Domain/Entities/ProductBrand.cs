using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductBrand: BaseEntity
{
    [StringLength(255)]
    public required string Name { get; set; }
    public required Address Address { get; set; }
}