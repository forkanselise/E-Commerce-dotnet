using System.Threading.Tasks;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;

namespace NexusBakery.Application.Services;

public class ProductService : IProductService
{
    public Task<string> CreateAsync(CreateProductDto dto) => Task.FromResult("id");
    public Task UpdateAsync(string id, UpdateProductDto dto) => Task.CompletedTask;
    public Task AddImageAsync(string id, string url) => Task.CompletedTask;
}
