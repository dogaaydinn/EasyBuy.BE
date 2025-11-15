using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
}
