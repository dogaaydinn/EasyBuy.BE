using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Categories;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Query to get a single category by ID.
/// Includes parent category information.
/// </summary>
public sealed class GetCategoryByIdQuery : IRequest<Result<CategoryDto>>
{
    public Guid CategoryId { get; }

    public GetCategoryByIdQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
