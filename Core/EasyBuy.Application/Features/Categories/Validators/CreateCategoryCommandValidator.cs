using EasyBuy.Application.Features.Categories.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Categories.Validators;

/// <summary>
/// Validator for CreateCategoryCommand.
/// Ensures category name, display order, and optional image URL are valid.
/// </summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MinimumLength(2)
            .WithMessage("Category name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl)
            .WithMessage("Image URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be 0 or greater")
            .LessThanOrEqualTo(9999)
            .WithMessage("Display order cannot exceed 9999");

        RuleFor(x => x.ParentCategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Parent category ID cannot be empty GUID")
            .When(x => x.ParentCategoryId.HasValue);
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
