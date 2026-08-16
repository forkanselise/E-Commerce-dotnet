using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public ShippingAddress ShippingAddress { get; set; } = new();
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cod;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderPricing Pricing { get; set; } = new();
    public PaymentMethodType PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ShippingAddress? ShippingAddress { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public List<OrderStatusHistoryItem> StatusHistory { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
}
