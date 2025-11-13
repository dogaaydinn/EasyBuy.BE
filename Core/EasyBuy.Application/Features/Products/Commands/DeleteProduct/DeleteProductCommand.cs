using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }

    public DeleteProductCommand(Guid id)
    {
        Id = id;
    }
}
