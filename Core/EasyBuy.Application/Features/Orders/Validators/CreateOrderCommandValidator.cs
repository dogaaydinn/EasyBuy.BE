using EasyBuy.Application.Features.Orders.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Orders.Validators;

/// <summary>
/// Validator for CreateOrderCommand.
/// Ensures order data is valid before processing.
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // Items validation
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleFor(x => x.Items)
            .Must(items => items.Count <= 50)
            .WithMessage("Order cannot contain more than 50 items");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Product ID is required");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than 0")
                    .LessThanOrEqualTo(100)
                    .WithMessage("Quantity cannot exceed 100 per item");
            });

        // Shipping address validation
        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required")
            .ChildRules(address =>
            {
                address.RuleFor(x => x.FullName)
                    .NotEmpty()
                    .WithMessage("Full name is required")
                    .MaximumLength(100)
                    .WithMessage("Full name cannot exceed 100 characters");

                address.RuleFor(x => x.AddressLine1)
                    .NotEmpty()
                    .WithMessage("Address line 1 is required")
                    .MaximumLength(200)
                    .WithMessage("Address line 1 cannot exceed 200 characters");

                address.RuleFor(x => x.City)
                    .NotEmpty()
                    .WithMessage("City is required")
                    .MaximumLength(100)
                    .WithMessage("City cannot exceed 100 characters");

                address.RuleFor(x => x.State)
                    .NotEmpty()
                    .WithMessage("State/Province is required")
                    .MaximumLength(100)
                    .WithMessage("State cannot exceed 100 characters");

                address.RuleFor(x => x.PostalCode)
                    .NotEmpty()
                    .WithMessage("Postal code is required")
                    .MaximumLength(20)
                    .WithMessage("Postal code cannot exceed 20 characters");

                address.RuleFor(x => x.Country)
                    .NotEmpty()
                    .WithMessage("Country is required")
                    .MaximumLength(100)
                    .WithMessage("Country cannot exceed 100 characters");

                address.RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .WithMessage("Phone number is required")
                    .Matches(@"^\+?[1-9]\d{1,14}$")
                    .WithMessage("Invalid phone number format");
            });

        // Billing address validation
        RuleFor(x => x.BillingAddress)
            .NotNull()
            .WithMessage("Billing address is required");

        // Payment method validation
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .Must(x => new[] { "CreditCard", "PayPal", "BankTransfer", "CashOnDelivery" }.Contains(x))
            .WithMessage("Invalid payment method. Valid options: CreditCard, PayPal, BankTransfer, CashOnDelivery");

        // Notes validation (optional)
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}
