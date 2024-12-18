using EasyBuy.Application.Abstractions.Storage;
using Microsoft.AspNetCore.Http;

namespace EasyBuy.Infrastructure.Services.Storage;

public class StorageService : IStorageService
{
    private readonly IStorage _storage;

    public StorageService(IStorage storage)
    {
        _storage = storage;
    }

    public string StorageName => _storage.GetType().Name;

    public async Task DeleteFileAsync(string path, string fileName)
    {
        await _storage.DeleteFileAsync(path, fileName);
    }

    public async Task<(string Folder, string Name)> UploadFileAsync(string path, IFormFile file, bool useGuid = true)
    {
        return await _storage.UploadFileAsync(path, file, useGuid);
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        return await _storage.GetFilesAsync(path);
    }

    public async Task<bool> HasFileAsync(string path, string fileName)
    {
        return await _storage.HasFileAsync(path, fileName);
    }

    public async Task<IEnumerable<(string Folder, string Name)>> UploadFilesAsync(string path,
        IFormFileCollection files, bool useGuid = true)
    {
        return await _storage.UploadFilesAsync(path, files, useGuid);
    }

    public string StrorageName { get; }

    public bool FileExists(string directoryPath, string fileName)
    {
        throw new NotImplementedException();
    }
}