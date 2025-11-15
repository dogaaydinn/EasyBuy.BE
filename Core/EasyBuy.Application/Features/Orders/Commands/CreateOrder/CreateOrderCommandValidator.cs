using FluentValidation;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count > 0).WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item");
        });

        RuleFor(x => x.DeliveryMethodId)
            .NotEmpty().WithMessage("Delivery method is required");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required")
            .ChildRules(address =>
            {
                address.RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("First name is required")
                    .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

                address.RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("Last name is required")
                    .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

                address.RuleFor(x => x.Street)
                    .NotEmpty().WithMessage("Street address is required")
                    .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters");

                address.RuleFor(x => x.City)
                    .NotEmpty().WithMessage("City is required")
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

                address.RuleFor(x => x.State)
                    .NotEmpty().WithMessage("State is required")
                    .MaximumLength(50).WithMessage("State cannot exceed 50 characters");

                address.RuleFor(x => x.ZipCode)
                    .NotEmpty().WithMessage("Zip code is required")
                    .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Invalid zip code format");

                address.RuleFor(x => x.Country)
                    .NotEmpty().WithMessage("Country is required")
                    .MaximumLength(50).WithMessage("Country cannot exceed 50 characters");
            });

        RuleFor(x => x.CouponCode)
            .MaximumLength(20).WithMessage("Coupon code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
