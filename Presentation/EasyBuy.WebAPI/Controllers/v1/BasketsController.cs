using Asp.Versioning;
using EasyBuy.Application.Features.Baskets.Commands;
using EasyBuy.Application.Features.Baskets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyBuy.WebAPI.Controllers.v1;

/// <summary>
/// Baskets management API endpoints.
/// Provides shopping basket operations with Redis storage and 30-day expiration.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class BasketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BasketsController> _logger;

    public BasketsController(IMediator mediator, ILogger<BasketsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's basket.
    /// Returns empty basket if none exists.
    /// </summary>
    /// <returns>User's basket</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBasket()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        _logger.LogInformation("Getting basket for user: {UserId}", userId);

        var query = new GetBasketQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add item to basket or update quantity if already exists.
    /// </summary>
    /// <param name="command">Product ID and quantity to add</param>
    /// <returns>Updated basket</returns>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToBasket([FromBody] AddToBasketCommand command)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        _logger.LogInformation("Adding to basket: User={UserId}, Product={ProductId}, Quantity={Quantity}",
            userId, command.ProductId, command.Quantity);

        command.UserId = userId;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update basket item quantity.
    /// </summary>
    /// <param name="basketItemId">Basket item ID</param>
    /// <param name="command">New quantity</param>
    /// <returns>Updated basket</returns>
    [HttpPut("items/{basketItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBasketItem(Guid basketItemId, [FromBody] UpdateBasketItemCommand command)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        _logger.LogInformation("Updating basket item: User={UserId}, ItemId={BasketItemId}, Quantity={Quantity}",
            userId, basketItemId, command.Quantity);

        command.UserId = userId;
        command.BasketItemId = basketItemId;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove item from basket.
    /// </summary>
    /// <param name="basketItemId">Basket item ID to remove</param>
    /// <returns>Updated basket</returns>
    [HttpDelete("items/{basketItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromBasket(Guid basketItemId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        _logger.LogInformation("Removing from basket: User={UserId}, ItemId={BasketItemId}", userId, basketItemId);

        var command = new RemoveFromBasketCommand
        {
            UserId = userId,
            BasketItemId = basketItemId
        };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Clear entire basket (remove all items).
    /// </summary>
    /// <returns>Success result</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearBasket()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        _logger.LogInformation("Clearing basket: User={UserId}", userId);

        var command = new ClearBasketCommand { UserId = userId };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userGuid)
            ? Guid.Empty
            : userGuid;
    }
}
