using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Command to update an existing category.
/// </summary>
public sealed class UpdateCategoryCommand : IRequest<Result<bool>>
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
