using AutoMapper;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Events;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly EasyBuyDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        EasyBuyDbContext context,
        UserManager<AppUser> userManager,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IMapper mapper,
        IPublisher publisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _mapper = mapper;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<OrderDto>.Failure("User not authenticated");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return Result<OrderDto>.Failure("User not found");
            }

            // Validate products and stock
            var productIds = request.Items.Select(x => x.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive && !p.IsDeleted)
                .ToListAsync(cancellationToken);

            if (products.Count != productIds.Count)
            {
                return Result<OrderDto>.Failure("One or more products not found or inactive");
            }

            // Check stock availability
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                if (product.Stock < item.Quantity)
                {
                    return Result<OrderDto>.Failure($"Insufficient stock for product: {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
                }
            }

            // Calculate order total
            decimal subtotal = 0;
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                subtotal += product.Price * item.Quantity;
            }

            decimal discount = 0;
            Guid? couponId = null;

            // Apply coupon if provided
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == request.CouponCode
                        && c.IsActive
                        && c.StartDate <= DateTime.UtcNow
                        && c.EndDate >= DateTime.UtcNow
                        && (c.UsageLimit == null || c.UsageCount < c.UsageLimit), cancellationToken);

                if (coupon != null)
                {
                    if (subtotal >= coupon.MinimumOrderAmount)
                    {
                        couponId = coupon.Id;

                        switch (coupon.DiscountType)
                        {
                            case DiscountType.Percentage:
                                discount = subtotal * (coupon.DiscountValue / 100);
                                if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
                                {
                                    discount = coupon.MaximumDiscountAmount.Value;
                                }
                                break;
                            case DiscountType.FixedAmount:
                                discount = coupon.DiscountValue;
                                break;
                        }

                        // Update coupon usage
                        coupon.UsageCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Coupon {CouponCode} not applied. Minimum order amount {MinAmount} not met. Current: {CurrentAmount}",
                            request.CouponCode, coupon.MinimumOrderAmount, subtotal);
                    }
                }
            }

            decimal shippingCost = 10.00m; // Default shipping cost
            decimal tax = (subtotal - discount) * 0.1m; // 10% tax
            decimal total = subtotal - discount + shippingCost + tax;

            // Get shipping address
            string shippingAddress = "";
            if (request.AddressId.HasValue)
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == request.AddressId.Value && a.UserId == userId, cancellationToken);

                if (address != null)
                {
                    shippingAddress = $"{address.Street}, {address.City}, {address.State} {address.ZipCode}, {address.Country}";
                }
            }

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                User = user,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                Discount = discount,
                ShippingCost = shippingCost,
                Tax = tax,
                Total = total,
                ShippingAddress = shippingAddress,
                CouponId = couponId,
                Notes = request.Notes,
                CreatedDate = DateTime.UtcNow
            };

            // Create order items
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    Total = product.Price * item.Quantity
                };

                order.OrderItems.Add(orderItem);

                // Reduce product stock
                product.Stock -= item.Quantity;
                product.ModifiedDate = DateTime.UtcNow;

                // Publish inventory changed event
                await _publisher.Publish(new ProductInventoryChangedEvent(
                    product.Id,
                    product.Name,
                    product.Stock,
                    -item.Quantity), cancellationToken);
            }

            // Create payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Amount = total,
                PaymentType = request.PaymentType,
                Status = PaymentStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            order.Payments.Add(payment);

            // Save to database
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}", orderNumber, userId);

            // Process payment
            if (request.PaymentType == PaymentType.CreditCard || request.PaymentType == PaymentType.DebitCard)
            {
                try
                {
                    var paymentResult = await _paymentService.ProcessPaymentAsync(
                        order.Id,
                        total,
                        "USD",
                        request.PaymentDetails ?? "",
                        cancellationToken);

                    if (paymentResult)
                    {
                        payment.Status = PaymentStatus.Completed;
                        payment.TransactionId = Guid.NewGuid().ToString(); // In real implementation, use actual transaction ID
                        payment.ProcessedAt = DateTime.UtcNow;
                        order.Status = OrderStatus.Confirmed;

                        await _context.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Payment processed successfully for order {OrderNumber}", orderNumber);
                    }
                    else
                    {
                        payment.Status = PaymentStatus.Failed;
                        await _context.SaveChangesAsync(cancellationToken);

                        _logger.LogWarning("Payment failed for order {OrderNumber}", orderNumber);
                        return Result<OrderDto>.Failure("Payment processing failed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment for order {OrderNumber}", orderNumber);
                    payment.Status = PaymentStatus.Failed;
                    await _context.SaveChangesAsync(cancellationToken);
                    return Result<OrderDto>.Failure($"Payment error: {ex.Message}");
                }
            }
            else
            {
                // For cash on delivery or other payment methods, confirm order immediately
                order.Status = OrderStatus.Confirmed;
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Publish order created event
            await _publisher.Publish(new OrderCreatedEvent(
                order.Id,
                orderNumber,
                userId,
                total,
                order.OrderItems.Count), cancellationToken);

            // Clear user's basket if exists
            var basket = await _context.Baskets
                .Include(b => b.BasketItems)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket != null)
            {
                _context.BasketItems.RemoveRange(basket.BasketItems);
                _context.Baskets.Remove(basket);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Map to DTO
            var orderDto = _mapper.Map<OrderDto>(order);

            return Result<OrderDto>.Success(orderDto, "Order created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<OrderDto>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}
