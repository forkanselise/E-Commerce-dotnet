using System.Threading.Tasks;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Domain.Entities;

namespace NexusBakery.Application.Interfaces;

public interface IProductService
{
    Task<object> GetAllAsync();
    Task<string> CreateAsync(CreateProductDto dto);
    Task<Product?> UpdateAsync(string id, UpdateProductDto dto);
    Task AddImageAsync(string id, string url);
}
