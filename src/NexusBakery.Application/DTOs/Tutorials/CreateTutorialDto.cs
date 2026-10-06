namespace NexusBakery.Application.DTOs.Tutorials;

public class CreateTutorialDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Cakes & Pastry";
    public string SkillLevel { get; set; } = "Beginner";
    public int DurationMinutes { get; set; } = 60;
    public decimal Price { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public string InstructorName { get; set; } = "Chef Instructor";
    public string InstructorBio { get; set; } = "Executive Pastry Chef";
    public string Thumbnail { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string AccessType { get; set; } = "SubscriberOnly";
}
