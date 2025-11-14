using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Command to delete a category.
/// Soft delete if category has products or subcategories, hard delete otherwise.
/// </summary>
public sealed class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid CategoryId { get; set; }
    public bool ForceDelete { get; set; } = false;
}
