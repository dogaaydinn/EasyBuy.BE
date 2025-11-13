using EasyBuy.Application.Features.Orders.Commands.CancelOrder;
using EasyBuy.Application.Features.Orders.Commands.CreateOrder;
using EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;
using EasyBuy.Application.Features.Orders.Queries.GetOrderById;
using EasyBuy.Application.Features.Orders.Queries.GetOrders;
using EasyBuy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

/// <summary>
/// Order management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
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
    /// Create a new order
    /// </summary>
    /// <param name="command">Order details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order details</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="400">Invalid request data or insufficient stock</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new order");

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Order creation failed: {Error}", result.Error);
            return BadRequest(result);
        }

        _logger.LogInformation("Order created successfully: {OrderId}", result.Data?.Id);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of orders
    /// </summary>
    /// <param name="query">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving orders list");

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve orders: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order retrieved successfully</response>
    /// <response code="404">Order not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized to view this order</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving order: {OrderId}", id);

        var result = await _mediator.Send(new GetOrderByIdQuery { Id = id }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Order not found or access denied: {OrderId}", id);
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update order status (Admin/Manager only)
    /// </summary>
    /// <param name="command">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Order status updated successfully</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("status")]
    [Authorize(Policy = "RequireManagerRole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating order status for order: {OrderId}", command.OrderId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update order status: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Order cancelled successfully</response>
    /// <response code="400">Order cannot be cancelled</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized to cancel this order</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] string? reason, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order: {OrderId}", id);

        var result = await _mediator.Send(new CancelOrderCommand { OrderId = id, Reason = reason }, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to cancel order: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get order statistics (Admin/Manager only)
    /// </summary>
    /// <param name="fromDate">Start date for statistics</param>
    /// <param name="toDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("statistics")]
    [Authorize(Policy = "RequireManagerRole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken cancellationToken)
    {
        // TODO: Implement order statistics query
        _logger.LogInformation("Retrieving order statistics");

        return Ok(new
        {
            message = "Order statistics endpoint - To be implemented in Week 3",
            fromDate,
            toDate
        });
    }
}
