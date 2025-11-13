using FluentValidation;

namespace EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100).WithMessage("Tracking number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber));
    }
}
