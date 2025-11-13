using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Events;
using EasyBuy.Domain.Exceptions;
using MediatR;

namespace EasyBuy.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public UpdateProductCommandHandler(
        IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository,
        IMapper mapper,
        IPublisher publisher)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productReadRepository.GetByIdAsync(request.Id.ToString(), false);
        if (product == null)
            throw new NotFoundException("Product", request.Id);

        var oldQuantity = product.Quantity;

        if (request.Name != null) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.Quantity.HasValue) product.Quantity = request.Quantity.Value;
        if (request.ProductType.HasValue) product.ProductType = request.ProductType.Value;
        if (request.Brand != null) product.ProductBrand = request.Brand;

        _productWriteRepository.Update(product);
        await _productWriteRepository.SaveChangesAsync();

        // Publish inventory changed event if quantity changed
        if (oldQuantity != product.Quantity)
        {
            await _publisher.Publish(
                new ProductInventoryChangedEvent(product.Id, oldQuantity, product.Quantity, "Manual Update"),
                cancellationToken);
        }

        var productDto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(productDto, "Product updated successfully");
    }
}
