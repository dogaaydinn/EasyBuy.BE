using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EasyBuy.Infrastructure.Services.Storage.Local;

public class LocalStorage : ILocalStorage
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public StorageService(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }
    
    public Task<string> UploadFileAsync(string path, IFormFileCollection files, bool useGuid = true)
    {
        ArgumentNullException.ThrowIfNull(path, nameof(path));
        
        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, path);
        FileHelper.EnsureDirectoryExists(filePath);
        
        var file = files.FirstOrDefault();
        if (file == null) return Task.FromResult(string.Empty);
        
        var fileName = useGuid ? $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}" : file.FileName;
        filePath = Path.Combine(filePath, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);
        
        return Task.FromResult(fileName);
        
    }

    public Task<string> FileRenameAsync(string path)
    {
        
        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, path);
        FileHelper.EnsureDirectoryExists(filePath);
        
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(path)}";
        filePath = Path.Combine(filePath, fileName);
        
        return Task.FromResult(fileName);
        
    }

    public Task<bool> CopyFileAsync(string sourcePath, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(sourcePath, nameof(sourcePath));
        
        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, sourcePath);
        FileHelper.EnsureDirectoryExists(filePath);
        
        filePath = Path.Combine(filePath, file.FileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);
        
        return Task.FromResult(true);
    }
}