using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommand : IRequest<Result<bool>>
{
    public required Guid ReviewId { get; set; }
    public required int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
