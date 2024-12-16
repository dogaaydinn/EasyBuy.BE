using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EasyBuy.Infrastructure.Services.Storage.Local;

public class LocalStorage : Storage, ILocalStorage
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public LocalStorage(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }


    public string StrorageName { get; }


    public async Task<(string Folder, string Name)> UploadFileAsync(string path, IFormFile file, bool useGuid = true)
    {
        ArgumentNullException.ThrowIfNull(file);

        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, path);
        FileHelper.EnsureDirectoryExists(uploadsFolder);

        var fileName = await Task.FromResult(useGuid ? $"{Guid.NewGuid()}_{file.FileName}" : file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return (Path: path, Name: fileName);
    }

    public Task DeleteFileAsync(string path, string fileName)
    {
        ArgumentNullException.ThrowIfNull(path, nameof(path));

        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, path);
        FileHelper.DeleteFileIfExists(filePath);

        return Task.CompletedTask;
    }


    public async Task<bool> HasFileAsync(string path, string fileName)
    {
        var filePath = Path.Combine(path, fileName);
        return await Task.Run(() => File.Exists(filePath));
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        return await Task.Run(() => Directory.GetFiles(path).AsEnumerable());
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
}