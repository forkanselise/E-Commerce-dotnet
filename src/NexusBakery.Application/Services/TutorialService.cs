using System.Text.RegularExpressions;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.DTOs.Tutorials;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Application.Services;

public interface ITutorialService
{
    Task<PagedResult<TutorialDto>> GetTutorialsAsync(TutorialSearchRequest request, string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<List<TutorialDto>> GetPopularTutorialsAsync(int limit = 6, string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<TutorialDto?> GetTutorialBySlugAsync(string slug, string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<TutorialDto> CreateTutorialAsync(CreateTutorialRequest request, string createdByUserId, CancellationToken cancellationToken = default);
}

public class TutorialService : ITutorialService
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUserRepository _userRepository;

    public TutorialService(ITutorialRepository tutorialRepository, IUserRepository userRepository)
    {
        _tutorialRepository = tutorialRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<TutorialDto>> GetTutorialsAsync(TutorialSearchRequest request, string? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _tutorialRepository.SearchTutorialsAsync(
            request.Search,
            request.Category,
            request.SkillLevel,
            request.Page,
            request.PageSize,
            cancellationToken);

        var hasActiveSub = await CheckUserHasActiveSubscriptionAsync(currentUserId, cancellationToken);

        return new PagedResult<TutorialDto>
        {
            Items = items.Select(t => MapToDto(t, hasActiveSub)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<TutorialDto>> GetPopularTutorialsAsync(int limit = 6, string? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var items = await _tutorialRepository.GetPopularAsync(limit, cancellationToken);
        var hasActiveSub = await CheckUserHasActiveSubscriptionAsync(currentUserId, cancellationToken);
        return items.Select(t => MapToDto(t, hasActiveSub)).ToList();
    }

    public async Task<TutorialDto?> GetTutorialBySlugAsync(string slug, string? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var tutorial = await _tutorialRepository.GetBySlugAsync(slug, cancellationToken);
        if (tutorial == null) return null;

        await _tutorialRepository.IncrementViewCountAsync(tutorial.Id, cancellationToken);
        var hasActiveSub = await CheckUserHasActiveSubscriptionAsync(currentUserId, cancellationToken);
        return MapToDto(tutorial, hasActiveSub);
    }

    public async Task<TutorialDto> CreateTutorialAsync(CreateTutorialRequest request, string createdByUserId, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(request.Title);
        var existingSlug = await _tutorialRepository.GetBySlugAsync(slug, cancellationToken);
        if (existingSlug != null)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        var tutorial = new Tutorial
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Category = request.Category.Trim(),
            SkillLevel = request.SkillLevel,
            Description = request.Description.Trim(),
            Instructor = request.Instructor,
            DurationMinutes = request.DurationMinutes,
            VideoUrl = request.VideoUrl,
            PreviewVideoUrl = request.PreviewVideoUrl,
            Thumbnail = request.Thumbnail,
            PosterImage = request.PosterImage,
            AccessType = request.AccessType,
            OneTimePurchasePrice = request.OneTimePurchasePrice,
            Chapters = request.Chapters,
            Ingredients = request.Ingredients,
            RequiredToolIds = request.RequiredToolIds,
            Tags = request.Tags,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };

        var created = await _tutorialRepository.CreateAsync(tutorial, cancellationToken);
        return MapToDto(created, true);
    }

    private async Task<bool> CheckUserHasActiveSubscriptionAsync(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        if (user.Role is UserRole.Admin or UserRole.SystemAdmin) return true;

        return user.Subscription != null &&
               user.Subscription.IsActive &&
               user.Subscription.Tier != SubscriptionTier.FreeLearner;
    }

    private static string GenerateSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private static TutorialDto MapToDto(Tutorial t, bool hasAccess)
    {
        var isFree = t.AccessType.Equals("Free", StringComparison.OrdinalIgnoreCase);
        var userHasAccess = isFree || hasAccess;

        return new TutorialDto
        {
            Id = t.Id,
            Title = t.Title,
            Slug = t.Slug,
            Category = t.Category,
            SkillLevel = t.SkillLevel,
            Description = t.Description,
            Instructor = t.Instructor,
            DurationMinutes = t.DurationMinutes,
            Thumbnail = t.Thumbnail,
            PreviewVideoUrl = t.PreviewVideoUrl,
            FullVideoUrl = userHasAccess ? t.VideoUrl : null, // Omit URL if paywalled
            IsSubscriberOnly = !isFree,
            OneTimePurchasePrice = t.OneTimePurchasePrice,
            Currency = t.Currency,
            Chapters = t.Chapters,
            Ingredients = t.Ingredients,
            RequiredToolIds = t.RequiredToolIds,
            ViewCount = t.ViewCount,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            UserHasAccess = userHasAccess,
            CreatedAt = t.CreatedAt
        };
    }
}
