using EasyBuy.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EasyBuy.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public FileService(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task UploadFileAsync(string path, IFormFileCollection files)
    {
        var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }
        
        foreach (var file in files)
        {
            var fileNewName = await FileRenameAsync(file.FileName);
            await CopyFileAsync(Path.Combine(uploadPath, fileNewName), file);
        }
    }

    public Task<string> FileRenameAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CopyFileAsync(string sourcePath, IFormFile file)
    {
        throw new NotImplementedException();
    }
}