using Microsoft.AspNetCore.Http;

namespace EasyBuy.Application.Abstractions.Storage;

public interface IStorage
{
    Task<List<(string fileName, string content)>> UploadAsync(string directoryPath, IFormFileCollection files);
    
    Task DeleteAsync(string directoryPath, string fileName);
    List<string> GetFiles(string directoryPath);
    bool FileExists(string directoryPath, string fileName);
}