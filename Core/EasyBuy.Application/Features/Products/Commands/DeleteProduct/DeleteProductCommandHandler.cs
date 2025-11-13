using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Domain.Exceptions;
using MediatR;

namespace EasyBuy.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;

    public DeleteProductCommandHandler(
        IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productReadRepository.GetByIdAsync(request.Id.ToString(), false);
        if (product == null)
            throw new NotFoundException("Product", request.Id);

        _productWriteRepository.SoftDelete(product);
        await _productWriteRepository.SaveChangesAsync();

        return Result.Success("Product deleted successfully");
    }
}
