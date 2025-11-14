using EasyBuy.Application.Features.Baskets.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Baskets.Validators;

/// <summary>
/// Validator for AddToBasketCommand.
/// Ensures product ID and quantity are valid.
/// </summary>
public sealed class AddToBasketCommandValidator : AbstractValidator<AddToBasketCommand>
{
    public AddToBasketCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty)
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100");
    }
}
