using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using MediatR;

namespace EasyBuy.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}
