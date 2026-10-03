using System.IO;
using System.Threading.Tasks;

namespace NexusBakery.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder);
}
