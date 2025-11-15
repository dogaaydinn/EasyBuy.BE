using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Orders.DTOs;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderWriteRepository _orderRepository;
    private readonly IProductReadRepository _productRepository;
    private readonly IReadRepository<Delivery> _deliveryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderWriteRepository orderRepository,
        IProductReadRepository productRepository,
        IReadRepository<Delivery> deliveryRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _deliveryRepository = deliveryRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<OrderDto>.Failure("User not authenticated");
            }

            // Validate delivery method
            var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryMethodId);
            if (delivery == null)
            {
                return Result<OrderDto>.Failure("Invalid delivery method");
            }

            // Create order items and validate inventory
            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return Result<OrderDto>.Failure($"Product {item.ProductId} not found");
                }

                if (product.Quantity < item.Quantity)
                {
                    return Result<OrderDto>.Failure($"Insufficient stock for product {product.Name}. Available: {product.Quantity}, Requested: {item.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    ProductName = product.Name
                };

                orderItems.Add(orderItem);
                subtotal += orderItem.TotalPrice;

                // Reduce inventory
                product.Quantity -= item.Quantity;
            }

            // Calculate totals
            var taxRate = 0.10m; // 10% tax (should be configurable)
            var taxAmount = subtotal * taxRate;
            var total = subtotal + taxAmount + delivery.Price;

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                ShippingCost = delivery.Price,
                DiscountAmount = 0, // Apply coupon logic here if needed
                Total = total,
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                AppUserId = userId,
                AppUser = null!, // Will be loaded by EF Core
                DeliveryId = delivery.Id,
                Delivery = delivery,
                Notes = request.Notes,
                OrderItems = orderItems
            };

            // Raise domain event
            order.RaiseDomainEvent(new OrderCreatedEvent(
                order.Id,
                userId,
                _currentUserService.UserEmail ?? "",
                _currentUserService.PhoneNumber,
                total,
                orderItems.Count));

            // Save order
            await _orderRepository.AddAsync(order);

            _logger.LogInformation(
                "Order {OrderNumber} created successfully for user {UserId}. Total: {Total}",
                orderNumber, userId, total);

            var orderDto = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<OrderDto>.Failure($"Failed to create order: {ex.Message}");
        }
    }
}
