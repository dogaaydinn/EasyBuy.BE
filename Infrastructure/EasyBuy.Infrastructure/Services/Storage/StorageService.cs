using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EasyBuy.Infrastructure.Services.Storage;

public class StorageService : IStorageService
{
    public Task<List<(string fileName, string content)>> UploadAsync(string directoryPath, IFormFileCollection files)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string directoryPath, string fileName)
    {
        throw new NotImplementedException();
    }

    public List<string> GetFiles(string directoryPath)
    {
        throw new NotImplementedException();
    }

    public bool FileExists(string directoryPath, string fileName)
    {
        throw new NotImplementedException();
    }
}