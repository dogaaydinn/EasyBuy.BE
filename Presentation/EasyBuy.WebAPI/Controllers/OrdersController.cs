using Asp.Versioning;
using EasyBuy.Application.Features.Orders.Commands.CancelOrder;
using EasyBuy.Application.Features.Orders.Commands.CreateOrder;
using EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;
using EasyBuy.Application.Features.Orders.DTOs;
using EasyBuy.Application.Features.Orders.Queries.GetOrderById;
using EasyBuy.Application.Features.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

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
    /// Get all orders (admin can see all, users see only their own)
    /// </summary>
    /// <param name="query">Query parameters including pagination and filters</param>
    /// <returns>Paged list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="command">Order creation details</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="dto">Status update details</param>
    /// <returns>Success result</returns>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            Status = dto.Status,
            TrackingNumber = dto.TrackingNumber,
            Notes = dto.Notes
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { message = "Order status updated successfully" }) : BadRequest(result.Error);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        var command = new CancelOrderCommand
        {
            OrderId = id,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { message = "Order cancelled successfully" }) : BadRequest(result.Error);
    }
}

public record CancelOrderRequest(string Reason);
public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
