using EasyBuy.Application.Features.Reviews.Commands;
using FluentValidation;

namespace EasyBuy.Application.Features.Reviews.Validators;

/// <summary>
/// Validator for UpdateReviewCommand.
/// Ensures updated review data is valid.
/// </summary>
public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEqual(Guid.Empty)
            .WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Comment)
            .MinimumLength(10)
            .WithMessage("Comment must be at least 10 characters if provided")
            .MaximumLength(2000)
            .WithMessage("Comment cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
