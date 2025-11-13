using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommand : IRequest<Result<ReviewDto>>
{
    public required Guid ProductId { get; set; }
    public required int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
