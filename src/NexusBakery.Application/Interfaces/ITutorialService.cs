using System.Threading.Tasks;
using System.Collections.Generic;
using NexusBakery.Application.DTOs.Tutorials;
using NexusBakery.Domain.Entities;

namespace NexusBakery.Application.Interfaces;

public interface ITutorialService
{
    Task<object> GetAllAsync();
    Task<Tutorial?> GetByIdAsync(string id);
    Task<Tutorial> CreateAsync(CreateTutorialDto dto);
    Task<Tutorial?> UpdateAsync(string id, UpdateTutorialDto dto);
    Task AddMediaAsync(string id, string url, bool isVideo);
}
