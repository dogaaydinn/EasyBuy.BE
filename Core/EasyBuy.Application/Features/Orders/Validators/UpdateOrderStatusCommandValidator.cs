using EasyBuy.Application.Features.Orders.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Orders.Validators;

/// <summary>
/// Validator for UpdateOrderStatusCommand.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    private static readonly string[] ValidStatuses = { "Created", "Processing", "Shipped", "Delivered", "Cancelled" };

    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Invalid status. Valid statuses: {string.Join(", ", ValidStatuses)}");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber))
            .WithMessage("Tracking number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}
