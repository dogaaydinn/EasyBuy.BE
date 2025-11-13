using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Exceptions;
using MediatR;

namespace EasyBuy.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetProductByIdQueryHandler(
        IProductReadRepository productReadRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _productReadRepository = productReadRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.Id}";

        // Try get from cache
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedProduct != null)
        {
            return Result<ProductDto>.Success(cachedProduct);
        }

        var product = await _productReadRepository.GetByIdAsync(request.Id.ToString(), false);
        if (product == null)
            throw new NotFoundException("Product", request.Id);

        var productDto = _mapper.Map<ProductDto>(product);

        // Cache the result for 15 minutes
        await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(15), cancellationToken);

        return Result<ProductDto>.Success(productDto);
    }
}
