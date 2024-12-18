using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EasyBuy.Infrastructure.Services.Storage.Azure;

public class AzureStorage : IStorage, IAzureStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureStorage(IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(configuration["Storage:Azure"]);
    }

    public async Task DeleteFileAsync(string containerName, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = containerClient.GetBlobs();
        return await Task.FromResult(blobs.Select(blob => blob.Name));
    }

    public async Task<bool> HasFileAsync(string containerName, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        return await blobClient.ExistsAsync();
    }

    public async Task<(string Folder, string Name)> UploadFileAsync(string containerName, IFormFile file,
        bool useGuid = true)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Dosya boş veya null.", nameof(file));

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName =
            useGuid ? $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}" : Path.GetFileName(file.FileName);
        var blobClient = containerClient.GetBlobClient(fileName);

        try
        {
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
            throw;
        }

        return (Folder: containerName, Name: fileName);
    }

    public async Task<IEnumerable<(string Folder, string Name)>> UploadFilesAsync(string containerName,
        IFormFileCollection files, bool useGuid = true)
    {
        if (files == null || files.Count == 0)
            throw new ArgumentException("Dosya koleksiyonu boş veya null.", nameof(files));

        var filePaths = new List<(string Path, string Name)>();
        foreach (var file in files)
        {
            var uploadedPath = await UploadFileAsync(containerName, file, useGuid);
            filePaths.Add(uploadedPath);
        }

        return filePaths;
    }
}