using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class Order : BaseEntity
{
    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("orderType")]
    public OrderType OrderType { get; set; } = OrderType.ECommerce;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new();

    [BsonElement("pricing")]
    public OrderPricing Pricing { get; set; } = new();

    [BsonElement("paymentMethod")]
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cod;

    [BsonElement("paymentStatus")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [BsonElement("paymentDetails")]
    public OrderPaymentDetails? PaymentDetails { get; set; }

    [BsonElement("shippingAddress")]
    public ShippingAddress? ShippingAddress { get; set; }

    [BsonElement("orderStatus")]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Placed;

    [BsonElement("statusHistory")]
    public List<OrderStatusHistoryItem> StatusHistory { get; set; } = new();

    [BsonElement("notes")]
    public string? Notes { get; set; }
}

public class OrderItem
{
    [BsonElement("productId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; } = 1;

    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }

    [BsonElement("thumbnail")]
    public string? Thumbnail { get; set; }
}

public class OrderPricing
{
    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }

    [BsonElement("shippingCost")]
    public decimal ShippingCost { get; set; } = 0;

    [BsonElement("discount")]
    public decimal Discount { get; set; } = 0;

    [BsonElement("couponCode")]
    public string? CouponCode { get; set; }

    [BsonElement("tax")]
    public decimal Tax { get; set; } = 0;

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";
}

public class OrderPaymentDetails
{
    [BsonElement("transactionId")]
    public string? TransactionId { get; set; }

    [BsonElement("gatewayName")]
    public string? GatewayName { get; set; }

    [BsonElement("gatewayResponse")]
    public string? GatewayResponse { get; set; }

    [BsonElement("paidAt")]
    public DateTime? PaidAt { get; set; }
}

public class OrderStatusHistoryItem
{
    [BsonElement("status")]
    public OrderStatus Status { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("note")]
    public string? Note { get; set; }
}
