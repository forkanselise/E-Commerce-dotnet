using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NexusBakery.Application.DTOs.Tutorials;
using NexusBakery.Application.Interfaces;
using NexusBakery.Domain.Interfaces;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Application.Services;

public class TutorialService : ITutorialService
{
    private readonly ITutorialRepository _tutorialRepository;

    public TutorialService(ITutorialRepository tutorialRepository)
    {
        _tutorialRepository = tutorialRepository;
    }

    public async Task<object> GetAllAsync()
    {
        return await _tutorialRepository.GetAllAsync();
    }

    public async Task<Tutorial?> GetByIdAsync(string id)
    {
        return await _tutorialRepository.GetByIdAsync(id);
    }

    public async Task<Tutorial> CreateAsync(CreateTutorialDto dto)
    {
        Enum.TryParse<SkillLevel>(dto.SkillLevel, true, out var levelEnum);

        var tutorial = new Tutorial
        {
            Title = dto.Title,
            Slug = dto.Title != null ? dto.Title.ToLower().Replace(" ", "-") : "new-tutorial",
            Category = dto.Category ?? "Cakes & Pastry",
            SkillLevel = levelEnum,
            DurationMinutes = dto.DurationMinutes,
            OneTimePurchasePrice = dto.Price,
            Description = dto.Description,
            Instructor = new TutorialInstructor
            {
                Name = dto.InstructorName ?? "Chef Instructor",
                Bio = dto.InstructorBio ?? "Executive Pastry Chef",
                AvatarUrl = "https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=150"
            },
            Thumbnail = dto.Thumbnail,
            VideoUrl = dto.VideoUrl,
            AccessType = dto.AccessType ?? "SubscriberOnly",
            IsPublished = true
        };

        return await _tutorialRepository.CreateAsync(tutorial);
    }

    public async Task<Tutorial?> UpdateAsync(string id, UpdateTutorialDto dto)
    {
        var existing = await _tutorialRepository.GetByIdAsync(id);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title)) existing.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Category)) existing.Category = dto.Category;
        if (!string.IsNullOrWhiteSpace(dto.Description)) existing.Description = dto.Description;
        if (dto.Thumbnail != null) existing.Thumbnail = dto.Thumbnail;
        if (!string.IsNullOrWhiteSpace(dto.VideoUrl)) existing.VideoUrl = dto.VideoUrl;
        if (!string.IsNullOrWhiteSpace(dto.AccessType)) existing.AccessType = dto.AccessType;
        if (dto.DurationMinutes > 0) existing.DurationMinutes = dto.DurationMinutes;
        if (dto.Price >= 0) existing.OneTimePurchasePrice = dto.Price;

        if (!string.IsNullOrWhiteSpace(dto.SkillLevel) && Enum.TryParse<SkillLevel>(dto.SkillLevel, true, out var levelEnum))
        {
            existing.SkillLevel = levelEnum;
        }

        if (!string.IsNullOrWhiteSpace(dto.InstructorName))
        {
            existing.Instructor.Name = dto.InstructorName;
        }
        if (!string.IsNullOrWhiteSpace(dto.InstructorBio))
        {
            existing.Instructor.Bio = dto.InstructorBio;
        }

        await _tutorialRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task AddMediaAsync(string id, string url, bool isVideo)
    {
        var existing = await _tutorialRepository.GetByIdAsync(id);
        if (existing != null)
        {
            if (isVideo) existing.VideoUrl = url;
            else existing.Thumbnail = url;
            await _tutorialRepository.UpdateAsync(existing);
        }
    }
}
