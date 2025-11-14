using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Categories;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Query to get hierarchical category tree.
/// Returns root categories with all nested subcategories.
/// </summary>
public sealed class GetCategoryTreeQuery : IRequest<Result<List<CategoryDto>>>
{
    public bool IncludeInactive { get; set; } = false;
}
