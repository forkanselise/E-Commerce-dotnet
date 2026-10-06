namespace NexusBakery.Application.DTOs.Tutorials;

public class UpdateTutorialDto
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? SkillLevel { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? InstructorName { get; set; }
    public string? InstructorBio { get; set; }
    public string? Thumbnail { get; set; }
    public string? VideoUrl { get; set; }
    public string? AccessType { get; set; }
}
