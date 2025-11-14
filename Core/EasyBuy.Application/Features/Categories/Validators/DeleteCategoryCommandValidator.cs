using EasyBuy.Application.Features.Categories.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Categories.Validators;

/// <summary>
/// Validator for DeleteCategoryCommand.
/// Ensures category ID is valid.
/// </summary>
public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID is required");
    }
}
