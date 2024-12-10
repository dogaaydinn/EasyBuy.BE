namespace EasyBuy.Infrastructure.Helpers;

public static class FileHelper
{
    public static void EnsureDirectoryExists(string path)
    {
        if (Directory.Exists(path)) return;
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to create directory: {path}", ex);
        }

    }
}