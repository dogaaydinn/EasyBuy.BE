using Asp.Versioning;
using EasyBuy.Application.Features.Reviews.Commands;
using EasyBuy.Application.Features.Reviews.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyBuy.WebAPI.Controllers.v1;

/// <summary>
/// Reviews management API endpoints.
/// Provides CRUD operations for product reviews with CQRS pattern.
/// </summary>
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
    /// Get all reviews with pagination and filtering.
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>Paged list of reviews</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReviews([FromQuery] GetReviewsQuery query)
    {
        _logger.LogInformation("Getting reviews: Page={Page}, Size={Size}, ProductId={ProductId}",
            query.PageNumber, query.PageSize, query.ProductId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all reviews for a specific product.
    /// Includes average rating and rating distribution.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Product reviews with statistics</returns>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductReviews(
        Guid productId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting reviews for product: {ProductId}, Page={Page}, Size={Size}",
            productId, pageNumber, pageSize);

        var query = new GetProductReviewsQuery(productId)
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get review by ID.
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>Review details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReviewById(Guid id)
    {
        _logger.LogInformation("Getting review: {ReviewId}", id);

        var query = new GetReviewByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new review.
    /// Users can only review each product once.
    /// </summary>
    /// <param name="command">Review creation data</param>
    /// <returns>Created review ID</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
    {
        _logger.LogInformation("Creating review for product: {ProductId}, Rating: {Rating}",
            command.ProductId, command.Rating);

        // Set user ID from claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        command.UserId = userGuid;

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetReviewById),
            new { id = result.Data },
            result);
    }

    /// <summary>
    /// Update an existing review.
    /// Users can only update their own reviews.
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="command">Review update data</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewCommand command)
    {
        _logger.LogInformation("Updating review: {ReviewId}", id);

        // Set user ID from claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        command.ReviewId = id;
        command.UserId = userGuid;

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a review.
    /// Users can only delete their own reviews, admins can delete any review.
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        _logger.LogInformation("Deleting review: {ReviewId}", id);

        // Set user ID from claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var isAdmin = User.IsInRole("Admin");

        var command = new DeleteReviewCommand
        {
            ReviewId = id,
            UserId = userGuid,
            IsAdmin = isAdmin
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
