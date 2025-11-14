using EasyBuy.Application.Features.Baskets.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Baskets.Validators;

/// <summary>
/// Validator for UpdateBasketItemCommand.
/// Ensures basket item ID and quantity are valid.
/// </summary>
public sealed class UpdateBasketItemCommandValidator : AbstractValidator<UpdateBasketItemCommand>
{
    public UpdateBasketItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.BasketItemId)
            .NotEqual(Guid.Empty)
            .WithMessage("Basket item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100");
    }
}
