using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;
using System.Linq.Expressions;

namespace NexusBakery.Application.Services;

public interface IMobilePhoneService
{
    Task<List<MobilePhone>> GetAllPhonesAsync(CancellationToken cancellationToken = default);
    Task<(List<MobilePhone> Items, long TotalCount)> GetPagedPhonesAsync(int page, int pageSize, string? search, string? brand, CancellationToken cancellationToken = default);
    Task<MobilePhone?> GetPhoneByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<MobilePhone> CreatePhoneAsync(MobilePhone phone, CancellationToken cancellationToken = default);
}

public class MobilePhoneService : IMobilePhoneService
{
    private readonly IMobilePhoneRepository _repository;

    public MobilePhoneService(IMobilePhoneRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MobilePhone>> GetAllPhonesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<(List<MobilePhone> Items, long TotalCount)> GetPagedPhonesAsync(int page, int pageSize, string? search, string? brand, CancellationToken cancellationToken = default)
    {
        Expression<Func<MobilePhone, bool>>? predicate = null;

        var searchLower = search?.ToLower() ?? string.Empty;
        var brandLower = brand?.ToLower() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(brand))
        {
            predicate = x => (x.Title.ToLower().Contains(searchLower) || 
                              x.Model.ToLower().Contains(searchLower)) && 
                              x.Brand.ToLower() == brandLower;
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            predicate = x => x.Title.ToLower().Contains(searchLower) || 
                             x.Model.ToLower().Contains(searchLower) ||
                             x.Brand.ToLower().Contains(searchLower);
        }
        else if (!string.IsNullOrWhiteSpace(brand))
        {
            predicate = x => x.Brand.ToLower() == brandLower;
        }

        return await _repository.GetPagedAsync(predicate, page, pageSize, cancellationToken: cancellationToken);
    }

    public async Task<MobilePhone?> GetPhoneByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<MobilePhone> CreatePhoneAsync(MobilePhone phone, CancellationToken cancellationToken = default)
    {
        return await _repository.CreateAsync(phone, cancellationToken);
    }
}
