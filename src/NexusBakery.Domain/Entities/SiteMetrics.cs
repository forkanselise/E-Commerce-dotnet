using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;

namespace NexusBakery.Domain.Entities;

public class SiteMetrics : BaseEntity
{
    [BsonElement("totalLifetimeVisits")]
    public long TotalLifetimeVisits { get; set; } = 0;

    [BsonElement("activeLiveUsers")]
    public int ActiveLiveUsers { get; set; } = 0;

    [BsonElement("totalRegisteredUsers")]
    public int TotalRegisteredUsers { get; set; } = 0;

    [BsonElement("totalOrdersCompleted")]
    public int TotalOrdersCompleted { get; set; } = 0;

    [BsonElement("totalRevenue")]
    public decimal TotalRevenue { get; set; } = 0;

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";

    [BsonElement("dailySnapshots")]
    public List<DailyMetricSnapshot> DailySnapshots { get; set; } = new();
}

public class DailyMetricSnapshot
{
    [BsonElement("date")]
    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

    [BsonElement("visits")]
    public int Visits { get; set; } = 0;

    [BsonElement("uniqueVisitors")]
    public int UniqueVisitors { get; set; } = 0;

    [BsonElement("newRegistrations")]
    public int NewRegistrations { get; set; } = 0;

    [BsonElement("ordersPlaced")]
    public int OrdersPlaced { get; set; } = 0;

    [BsonElement("revenue")]
    public decimal Revenue { get; set; } = 0;
}
