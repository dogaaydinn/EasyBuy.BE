using FluentValidation;

namespace EasyBuy.Application.Features.Baskets.Commands.UpdateBasketItemQuantity;

public class UpdateBasketItemQuantityCommandValidator : AbstractValidator<UpdateBasketItemQuantityCommand>
{
    public UpdateBasketItemQuantityCommandValidator()
    {
        RuleFor(x => x.BasketItemId)
            .NotEmpty().WithMessage("Basket item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");
    }
}
