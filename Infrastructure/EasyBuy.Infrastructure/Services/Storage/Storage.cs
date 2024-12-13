namespace EasyBuy.Infrastructure.Services.Storage;

public abstract class Storage
{
    public delegate Task<bool> HasFile(string path, string fileName);
}