using Asp.Versioning;
using EasyBuy.Application.Features.Orders.Commands;
using EasyBuy.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers.v1;

/// <summary>
/// Orders management API endpoints.
/// Provides CRUD operations for orders with CQRS pattern.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with pagination and filtering.
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>Paged list of orders</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
    {
        _logger.LogInformation("Getting orders: Page={Page}, Size={Size}", query.PageNumber, query.PageSize);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get order by ID.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("Getting order: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    /// <param name="command">Order creation data</param>
    /// <returns>Created order ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        _logger.LogInformation("Creating order with {ItemCount} items", command.Items.Count);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Data },
            result);
    }

    /// <summary>
    /// Update order status.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="command">Status update data</param>
    /// <returns>Success result</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        _logger.LogInformation("Updating order status: {OrderId} to {Status}", id, command.Status);

        command.OrderId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel an order.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="command">Cancellation data</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderCommand command)
    {
        _logger.LogInformation("Cancelling order: {OrderId}", id);

        command.OrderId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
