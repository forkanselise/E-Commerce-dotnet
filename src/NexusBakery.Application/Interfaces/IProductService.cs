using System.Threading.Tasks; using NexusBakery.Application.DTOs.Products; namespace NexusBakery.Application.Interfaces; public interface IProductService { Task<object> GetAllAsync();
    Task<string> CreateAsync(CreateProductDto dto); Task UpdateAsync(string id, UpdateProductDto dto); Task AddImageAsync(string id, string url); }
