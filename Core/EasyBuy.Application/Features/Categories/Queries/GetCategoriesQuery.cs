using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Categories;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Query to get all categories with pagination and filtering.
/// Supports filtering by parent category and active status.
/// </summary>
public sealed class GetCategoriesQuery : IRequest<Result<PagedResult<CategoryDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public string? OrderBy { get; set; } = "DisplayOrder";
    public bool Descending { get; set; } = false;
}
