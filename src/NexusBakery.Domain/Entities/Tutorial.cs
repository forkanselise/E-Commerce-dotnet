using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class Tutorial : BaseEntity
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = "Pastry & Macarons";

    [BsonElement("skillLevel")]
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("instructor")]
    public TutorialInstructor Instructor { get; set; } = new();

    [BsonElement("durationMinutes")]
    public int DurationMinutes { get; set; }

    [BsonElement("videoUrl")]
    public string VideoUrl { get; set; } = string.Empty;

    [BsonElement("previewVideoUrl")]
    public string PreviewVideoUrl { get; set; } = string.Empty;

    [BsonElement("thumbnail")]
    public string Thumbnail { get; set; } = string.Empty;

    [BsonElement("posterImage")]
    public string? PosterImage { get; set; }

    [BsonElement("chapters")]
    public List<VideoChapter> Chapters { get; set; } = new();

    [BsonElement("ingredients")]
    public List<string> Ingredients { get; set; } = new();

    [BsonElement("requiredToolIds")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> RequiredToolIds { get; set; } = new();

    [BsonElement("accessType")]
    public string AccessType { get; set; } = "SubscriberOnly"; // "Free", "SubscriberOnly", "Purchased"

    [BsonElement("oneTimePurchasePrice")]
    public decimal? OneTimePurchasePrice { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("viewCount")]
    public long ViewCount { get; set; } = 0;

    [BsonElement("averageRating")]
    public double AverageRating { get; set; } = 5.0;

    [BsonElement("reviewCount")]
    public int ReviewCount { get; set; } = 0;

    [BsonElement("isPublished")]
    public bool IsPublished { get; set; } = true;

    [BsonElement("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CreatedBy { get; set; }
}

public class TutorialInstructor
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("bio")]
    public string Bio { get; set; } = string.Empty;

    [BsonElement("avatarUrl")]
    public string AvatarUrl { get; set; } = string.Empty;
}

public class VideoChapter
{
    [BsonElement("timestampSeconds")]
    public int TimestampSeconds { get; set; }

    [BsonElement("timestampDisplay")]
    public string TimestampDisplay { get; set; } = "00:00";

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
}
