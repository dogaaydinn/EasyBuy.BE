using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Handler for CreateOrderCommand.
/// Creates order, reserves inventory, and triggers order creation events.
/// </summary>
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderWriteRepository _orderRepository;
    private readonly IProductReadRepository _productRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderWriteRepository orderRepository,
        IProductReadRepository productRepository,
        IDomainEventDispatcher eventDispatcher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order with {ItemCount} items", request.Items.Count);

        try
        {
            // Validate products exist and calculate totals
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return Result<Guid>.Failure($"Product not found: {item.ProductId}");
                }

                // Check stock availability
                if (product.Stock < item.Quantity)
                {
                    return Result<Guid>.Failure($"Insufficient stock for product: {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity
                };

                orderItems.Add(orderItem);
                subtotal += orderItem.TotalPrice;
            }

            // Calculate tax and shipping (simplified for demo)
            decimal taxRate = 0.10m; // 10% tax
            decimal tax = subtotal * taxRate;
            decimal shippingCost = subtotal > 100 ? 0 : 10m; // Free shipping over $100
            decimal totalAmount = subtotal + tax + shippingCost;

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                // UserId would come from current user service
                SubTotal = subtotal,
                Tax = tax,
                ShippingCost = shippingCost,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending",
                Status = "Created",
                OrderDate = DateTime.UtcNow,
                Items = orderItems,
                Notes = request.Notes
            };

            // Save order
            await _orderRepository.AddAsync(order);

            _logger.LogInformation("Order created successfully: {OrderId}, Total: {Total}", order.Id, order.TotalAmount);

            // Dispatch domain event
            var orderCreatedEvent = new OrderCreatedEvent(
                order.Id,
                order.UserId,
                "user@example.com", // TODO: Get from current user
                order.TotalAmount,
                "1234567890"); // TODO: Get phone from user

            await _eventDispatcher.DispatchAsync(orderCreatedEvent, cancellationToken);

            return Result<Guid>.Success(order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<Guid>.Failure($"Failed to create order: {ex.Message}");
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
