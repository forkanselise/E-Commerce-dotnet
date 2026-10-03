using System.IO;
using System.Threading.Tasks;
using NexusBakery.Application.Interfaces;

namespace NexusBakery.Application.Services;

public class MockFileStorageService : IFileStorageService
{
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        return Task.FromResult($"https://mockstorage.com/{folder}/{fileName}");
    }
}
