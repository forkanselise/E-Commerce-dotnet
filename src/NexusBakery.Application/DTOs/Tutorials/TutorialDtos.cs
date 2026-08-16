using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Application.DTOs.Tutorials;

public class TutorialDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public SkillLevel SkillLevel { get; set; }
    public string Description { get; set; } = string.Empty;
    public TutorialInstructor Instructor { get; set; } = new();
    public int DurationMinutes { get; set; }
    public string Thumbnail { get; set; } = string.Empty;
    public string PreviewVideoUrl { get; set; } = string.Empty;
    public string? FullVideoUrl { get; set; } // Omitted for unauthorized/unsubscribed users
    public bool IsSubscriberOnly { get; set; }
    public decimal? OneTimePurchasePrice { get; set; }
    public string Currency { get; set; } = "BDT";
    public List<VideoChapter> Chapters { get; set; } = new();
    public List<string> Ingredients { get; set; } = new();
    public List<string> RequiredToolIds { get; set; } = new();
    public long ViewCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool UserHasAccess { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TutorialSearchRequest
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public SkillLevel? SkillLevel { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class CreateTutorialRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Pastry & Macarons";
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;
    public string Description { get; set; } = string.Empty;
    public TutorialInstructor Instructor { get; set; } = new();
    public int DurationMinutes { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string PreviewVideoUrl { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public string? PosterImage { get; set; }
    public string AccessType { get; set; } = "SubscriberOnly";
    public decimal? OneTimePurchasePrice { get; set; }
    public List<VideoChapter> Chapters { get; set; } = new();
    public List<string> Ingredients { get; set; } = new();
    public List<string> RequiredToolIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}
