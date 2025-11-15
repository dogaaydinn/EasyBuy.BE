using Asp.Versioning;
using EasyBuy.Application.Features.Baskets.Commands.AddToBasket;
using EasyBuy.Application.Features.Baskets.Commands.ClearBasket;
using EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;
using EasyBuy.Application.Features.Baskets.DTOs;
using EasyBuy.Application.Features.Baskets.Queries.GetBasket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

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
    /// Get current user's basket
    /// </summary>
    /// <returns>Basket with items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBasket()
    {
        var result = await _mediator.Send(new GetBasketQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Add item to basket
    /// </summary>
    /// <param name="dto">Item to add</param>
    /// <returns>Updated basket</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToBasket([FromBody] AddToBasketDto dto)
    {
        var command = new AddToBasketCommand
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Remove item from basket
    /// </summary>
    /// <param name="productId">Product ID to remove</param>
    /// <returns>Updated basket</returns>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromBasket(Guid productId)
    {
        var command = new RemoveFromBasketCommand { ProductId = productId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Clear entire basket
    /// </summary>
    /// <returns>Success result</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearBasket()
    {
        var result = await _mediator.Send(new ClearBasketCommand());
        return result.IsSuccess ? Ok(new { message = "Basket cleared successfully" }) : BadRequest(result.Error);
    }
}
