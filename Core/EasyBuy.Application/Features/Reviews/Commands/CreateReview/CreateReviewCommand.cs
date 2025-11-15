using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Reviews.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand : IRequest<Result<ReviewDto>>
{
    public Guid ProductId { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
}
