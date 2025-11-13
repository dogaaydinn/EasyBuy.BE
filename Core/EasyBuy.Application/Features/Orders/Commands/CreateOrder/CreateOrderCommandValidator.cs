using FluentValidation;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items != null && items.Count > 0).WithMessage("Order items cannot be empty");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item");

            item.RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");
        });

        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("Invalid payment type");

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
