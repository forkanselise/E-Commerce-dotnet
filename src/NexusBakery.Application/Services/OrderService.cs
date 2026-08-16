using NexusBakery.Application.DTOs.Orders;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, string userId, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(string orderId, string userId, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IMetricsRepository _metricsRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IMetricsRepository metricsRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _metricsRepository = metricsRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, string userId, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemReq.ProductId, cancellationToken);
            if (product == null || !product.IsAvailable)
            {
                throw new InvalidOperationException($"Product '{itemReq.ProductId}' is no longer available.");
            }

            if (product.WarehouseStock < itemReq.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for '{product.Title}'. Requested: {itemReq.Quantity}, Available: {product.WarehouseStock}.");
            }

            var itemSubtotal = product.Price * itemReq.Quantity;
            subtotal += itemSubtotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Title = product.Title,
                Sku = product.Sku,
                UnitPrice = product.Price,
                Quantity = itemReq.Quantity,
                Subtotal = itemSubtotal,
                Thumbnail = product.Images.FirstOrDefault()?.Url
            });

            // Decrement Stock
            await _productRepository.UpdateStockAsync(product.Id, -itemReq.Quantity, cancellationToken);
            var newStock = product.WarehouseStock - itemReq.Quantity;
            await _warehouseRepository.AdjustStockAsync(
                product.Id,
                newStock,
                userId,
                "Customer Order",
                "User",
                $"Order fulfillment for {itemReq.Quantity} units",
                cancellationToken);
        }

        decimal shippingCost = subtotal > 2500 ? 0 : 120.00m; // Free shipping over 2500 BDT
        decimal discount = 0;
        if (!string.IsNullOrEmpty(request.CouponCode) && request.CouponCode.Trim().ToUpperInvariant() == "NEXUS10")
        {
            discount = subtotal * 0.10m; // 10% coupon discount
        }

        var totalAmount = subtotal + shippingCost - discount;

        var orderNumber = $"NB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            OrderType = OrderType.ECommerce,
            Items = orderItems,
            Pricing = new OrderPricing
            {
                Subtotal = subtotal,
                ShippingCost = shippingCost,
                Discount = discount,
                CouponCode = request.CouponCode,
                TotalAmount = totalAmount,
                Currency = "BDT"
            },
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = request.PaymentMethod == PaymentMethodType.Cod ? PaymentStatus.CodPending : PaymentStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            OrderStatus = OrderStatus.Placed,
            Notes = request.Notes,
            StatusHistory = new List<OrderStatusHistoryItem>
            {
                new()
                {
                    Status = OrderStatus.Placed,
                    Timestamp = DateTime.UtcNow,
                    Note = "Order placed successfully"
                }
            }
        };

        var created = await _orderRepository.CreateAsync(order, cancellationToken);
        await _metricsRepository.RecordOrderCompletedAsync(totalAmount, cancellationToken);

        return MapToDto(created);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string orderId, string userId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return null;

        if (!isAdmin && order.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this order.");
        }

        return MapToDto(order);
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.UpdateStatusAsync(orderId, request.Status, request.Note, cancellationToken);
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            UserId = o.UserId,
            OrderType = o.OrderType,
            Items = o.Items,
            Pricing = o.Pricing,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = o.PaymentStatus,
            ShippingAddress = o.ShippingAddress,
            OrderStatus = o.OrderStatus,
            StatusHistory = o.StatusHistory,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt
        };
    }
}
