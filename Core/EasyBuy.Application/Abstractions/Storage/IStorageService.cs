namespace EasyBuy.Application.Abstractions.Storage;

public interface IStorageService : IStorage
{
    string StorageName { get; }
}