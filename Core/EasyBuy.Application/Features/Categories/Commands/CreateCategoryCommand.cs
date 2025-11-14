using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Command to create a new category.
/// Supports hierarchical parent-child relationships.
/// </summary>
public sealed class CreateCategoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
