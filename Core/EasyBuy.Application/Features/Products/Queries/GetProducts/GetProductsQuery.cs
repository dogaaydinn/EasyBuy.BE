using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Domain.Enums;
using MediatR;

namespace EasyBuy.Application.Features.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<PagedResult<ProductListDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ProductType? ProductType { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}
