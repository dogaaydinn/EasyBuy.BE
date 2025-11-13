using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Domain.Enums;
using MediatR;

namespace EasyBuy.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Result<ProductDto>>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ProductType ProductType { get; set; }
    public string? Brand { get; set; }
    public Guid? CategoryId { get; set; }
}
