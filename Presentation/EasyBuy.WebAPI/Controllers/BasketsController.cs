using EasyBuy.Application.Features.Baskets.Commands.AddToBasket;
using EasyBuy.Application.Features.Baskets.Commands.ClearBasket;
using EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;
using EasyBuy.Application.Features.Baskets.Commands.UpdateBasketItemQuantity;
using EasyBuy.Application.Features.Baskets.Queries.GetBasket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

/// <summary>
/// Shopping basket management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
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
    /// Get current user's basket
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User's shopping basket</returns>
    /// <response code="200">Basket retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBasket(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user basket");

        var result = await _mediator.Send(new GetBasketQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve basket: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add item to basket
    /// </summary>
    /// <param name="command">Product and quantity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated basket</returns>
    /// <response code="200">Item added to basket successfully</response>
    /// <response code="400">Invalid request or insufficient stock</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToBasket([FromBody] AddToBasketCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding item to basket: ProductId={ProductId}, Quantity={Quantity}",
            command.ProductId, command.Quantity);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to add item to basket: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update basket item quantity
    /// </summary>
    /// <param name="itemId">Basket item ID</param>
    /// <param name="quantity">New quantity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item quantity updated successfully</response>
    /// <response code="400">Invalid request or insufficient stock</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Basket item not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBasketItemQuantity(Guid itemId, [FromBody] int quantity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating basket item {ItemId} quantity to {Quantity}", itemId, quantity);

        var result = await _mediator.Send(new UpdateBasketItemQuantityCommand
        {
            BasketItemId = itemId,
            Quantity = quantity
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update basket item quantity: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove item from basket
    /// </summary>
    /// <param name="itemId">Basket item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item removed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Basket item not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromBasket(Guid itemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item from basket: {ItemId}", itemId);

        var result = await _mediator.Send(new RemoveFromBasketCommand { BasketItemId = itemId }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove item from basket: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Clear all items from basket
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Basket cleared successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearBasket(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing user basket");

        var result = await _mediator.Send(new ClearBasketCommand(), cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to clear basket: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get basket item count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of items in basket</returns>
    /// <response code="200">Count retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBasketItemCount(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving basket item count");

        var result = await _mediator.Send(new GetBasketQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var count = result.Data?.ItemCount ?? 0;

        return Ok(new
        {
            isSuccess = true,
            data = new { count },
            message = "Basket item count retrieved"
        });
    }
}
