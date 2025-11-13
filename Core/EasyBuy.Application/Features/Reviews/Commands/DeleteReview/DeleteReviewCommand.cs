using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommand : IRequest<Result<bool>>
{
    public required Guid ReviewId { get; set; }
}
