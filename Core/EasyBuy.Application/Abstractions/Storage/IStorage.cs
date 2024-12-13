using Microsoft.AspNetCore.Http;

namespace EasyBuy.Application.Abstractions.Storage;

public interface IStorage
{
    Task<(string Folder, string Name)> UploadFileAsync(string path, IFormFile file, bool useGuid = true);
    Task<IEnumerable<(string Folder, string Name)>> UploadFilesAsync(string path, IFormFileCollection files, bool useGuid = true);
    Task DeleteFileAsync(string path, string fileName);
    Task<bool> HasFileAsync(string path, string fileName);
    Task<IEnumerable<string>> GetFilesAsync(string path);
}