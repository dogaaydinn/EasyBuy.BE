using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Events;
using MediatR;

namespace EasyBuy.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public CreateProductCommandHandler(
        IProductWriteRepository productWriteRepository,
        IMapper mapper,
        IPublisher publisher)
    {
        _productWriteRepository = productWriteRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Quantity = request.Quantity,
            ProductType = request.ProductType,
            ProductBrand = request.Brand ?? string.Empty
        };

        await _productWriteRepository.AddAsync(product);
        await _productWriteRepository.SaveChangesAsync();

        // Publish domain event
        await _publisher.Publish(new ProductCreatedEvent(product.Id, product.Name, product.Price), cancellationToken);

        var productDto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(productDto, "Product created successfully");
    }
}
