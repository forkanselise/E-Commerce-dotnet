using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class Subscription : BaseEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("tier")]
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.MasterclassPro;

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // "Active", "Cancelled", "Expired"

    [BsonElement("pricing")]
    public SubscriptionPricing Pricing { get; set; } = new();

    [BsonElement("paymentMethod")]
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Card;

    [BsonElement("currentPeriod")]
    public SubscriptionPeriod CurrentPeriod { get; set; } = new();

    [BsonElement("autoRenew")]
    public bool AutoRenew { get; set; } = true;

    [BsonElement("renewalHistory")]
    public List<SubscriptionRenewalRecord> RenewalHistory { get; set; } = new();

    [BsonElement("accessGrants")]
    public List<string> AccessGrants { get; set; } = new();

    [BsonElement("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [BsonElement("cancellationReason")]
    public string? CancellationReason { get; set; }
}

public class SubscriptionPricing
{
    [BsonElement("monthlyPrice")]
    public decimal MonthlyPrice { get; set; } = 799.00m;

    [BsonElement("annualPrice")]
    public decimal AnnualPrice { get; set; } = 7999.00m;

    [BsonElement("billingCycle")]
    public string BillingCycle { get; set; } = "Monthly"; // "Monthly" | "Annual"

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";
}

public class SubscriptionPeriod
{
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [BsonElement("endDate")]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
}

public class SubscriptionRenewalRecord
{
    [BsonElement("periodStart")]
    public DateTime PeriodStart { get; set; }

    [BsonElement("periodEnd")]
    public DateTime PeriodEnd { get; set; }

    [BsonElement("amountPaid")]
    public decimal AmountPaid { get; set; }

    [BsonElement("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    [BsonElement("paidAt")]
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
