using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class ProductType : BaseEntity
{
    [StringLength(127)]
    public required string Category { get; set; }
}