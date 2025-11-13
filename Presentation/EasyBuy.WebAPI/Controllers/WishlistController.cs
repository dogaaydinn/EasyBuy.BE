using EasyBuy.Application.Features.Wishlists.Commands.AddToWishlist;
using EasyBuy.Application.Features.Wishlists.Commands.RemoveFromWishlist;
using EasyBuy.Application.Features.Wishlists.Queries.GetWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

/// <summary>
/// Wishlist management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IMediator mediator, ILogger<WishlistController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's wishlist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wishlist items</returns>
    /// <response code="200">Wishlist retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user wishlist");

        var result = await _mediator.Send(new GetWishlistQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    /// <param name="command">Product ID to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Product added to wishlist</response>
    /// <response code="400">Product already in wishlist or not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product {ProductId} to wishlist", command.ProductId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to add to wishlist: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    /// <param name="id">Wishlist item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Product removed from wishlist</response>
    /// <response code="400">Wishlist item not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFromWishlist(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing wishlist item {WishlistId}", id);

        var result = await _mediator.Send(new RemoveFromWishlistCommand { WishlistId = id }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove from wishlist: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
