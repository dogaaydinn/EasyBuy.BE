using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Application.Repositories.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetProductsQueryHandler(
        IProductReadRepository productReadRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _productReadRepository = productReadRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _productReadRepository.Query;

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                    (p.Description != null && p.Description.Contains(request.SearchTerm)));
        }

        if (request.ProductType.HasValue)
        {
            query = query.Where(p => p.ProductType == request.ProductType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            query = query.Where(p => p.ProductBrand == request.Brand);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (request.InStockOnly == true)
        {
            query = query.Where(p => p.Quantity > 0);
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "date" => request.SortDescending ? query.OrderByDescending(p => EF.Property<DateTime>(p, "CreatedDate")) : query.OrderBy(p => EF.Property<DateTime>(p, "CreatedDate")),
            _ => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var productDtos = _mapper.Map<List<ProductListDto>>(products);

        return new PagedResult<ProductListDto>(productDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
