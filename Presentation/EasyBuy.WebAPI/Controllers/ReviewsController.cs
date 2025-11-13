using EasyBuy.Application.Features.Reviews.Commands.CreateReview;
using EasyBuy.Application.Features.Reviews.Commands.DeleteReview;
using EasyBuy.Application.Features.Reviews.Commands.UpdateReview;
using EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

/// <summary>
/// Product review management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
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
    /// Get reviews for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="rating">Filter by rating (1-5)</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="sortBy">Sort by: CreatedDate, Rating, Helpful</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of reviews</returns>
    [HttpGet("products/{productId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductReviews(
        Guid productId,
        [FromQuery] int? rating,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "CreatedDate",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductReviewsQuery
        {
            ProductId = productId,
            Rating = rating,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new review for a product
    /// </summary>
    /// <param name="command">Review details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review</returns>
    /// <response code="200">Review created successfully</response>
    /// <response code="400">Invalid request or user hasn't purchased product</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating review for product {ProductId}", command.ProductId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create review: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="command">Updated review details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Review updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized to update this review</response>
    /// <response code="404">Review not found</response>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ReviewId)
        {
            return BadRequest(new { error = "Review ID mismatch" });
        }

        _logger.LogInformation("Updating review {ReviewId}", id);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update review: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Review deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized to delete this review</response>
    /// <response code="404">Review not found</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting review {ReviewId}", id);

        var result = await _mediator.Send(new DeleteReviewCommand { ReviewId = id }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete review: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
