using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EasyBuy.Infrastructure.Services.Storage.Azure;

public class AzureStorage : Storage, IAzureStorage, IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;


    public AzureStorage(IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(configuration["Storage:Azure"]);
    }
    
    public async Task DeleteFileAsync(string path, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(path);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(path);
        var blobs = containerClient.GetBlobs();
        return await Task.FromResult(blobs.Select(blob => blob.Name));
    }

    public async Task<bool> HasFileAsync(string path, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(path);
        var blobClient = containerClient.GetBlobClient(fileName);
        return await blobClient.ExistsAsync();
    }
    
    public async Task<(string Folder, string Name)> UploadFileAsync(string path, IFormFile file, bool useGuid = true)
    {
        ArgumentNullException.ThrowIfNull(file);

        var containerClient = _blobServiceClient.GetBlobContainerClient(path);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName = useGuid ? $"{Guid.NewGuid()}_{file.FileName}" : file.FileName;
        var blobClient = containerClient.GetBlobClient(fileName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }

        return (Path: path, Name: fileName);
    }

    public async Task<IEnumerable<(string Folder, string Name)>> UploadFilesAsync(string path,
        IFormFileCollection files, bool useGuid = true)
    {
        ArgumentNullException.ThrowIfNull(files);

        var filePaths = new List<(string Path, string Name)>();
        foreach (var file in files)
        {
            var uploadedPath = await UploadFileAsync(path, file, useGuid);
            filePaths.Add(uploadedPath);
        }

        return filePaths;
    }

    public string StrorageName { get; }
}