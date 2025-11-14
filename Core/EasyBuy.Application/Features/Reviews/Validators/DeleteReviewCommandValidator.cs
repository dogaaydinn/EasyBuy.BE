using EasyBuy.Application.Features.Reviews.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Reviews.Validators;

/// <summary>
/// Validator for DeleteReviewCommand.
/// Ensures review ID is valid.
/// </summary>
public sealed class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEqual(Guid.Empty)
            .WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");
    }
}
