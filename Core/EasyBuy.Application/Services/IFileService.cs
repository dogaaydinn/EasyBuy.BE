using Microsoft.AspNetCore.Http;

namespace EasyBuy.Application.Services;

public interface IFileService
{
    Task UploadFileAsync( string path, IFormFileCollection files);
    Task<string> FileRenameAsync(string path);
    Task<bool> CopyFileAsync(string sourcePath, IFormFile file);
}