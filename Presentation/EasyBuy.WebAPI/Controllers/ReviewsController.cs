using Asp.Versioning;
using EasyBuy.Application.Features.Reviews.Commands.CreateReview;
using EasyBuy.Application.Features.Reviews.Commands.DeleteReview;
using EasyBuy.Application.Features.Reviews.DTOs;
using EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get reviews for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="minRating">Minimum rating filter (1-5)</param>
    /// <param name="verifiedOnly">Show only verified purchases</param>
    /// <returns>List of reviews</returns>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductReviews(
        Guid productId,
        [FromQuery] int? minRating = null,
        [FromQuery] bool? verifiedOnly = null)
    {
        var query = new GetProductReviewsQuery
        {
            ProductId = productId,
            MinRating = minRating,
            VerifiedOnly = verifiedOnly
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Create a product review
    /// </summary>
    /// <param name="command">Review details</param>
    /// <returns>Created review</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetProductReviews),
            new { productId = result.Data!.ProductId },
            result.Data);
    }

    /// <summary>
    /// Delete a review (own review or admin)
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var command = new DeleteReviewCommand { ReviewId = id };
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Review deleted successfully" })
            : BadRequest(result.Error);
    }
}
