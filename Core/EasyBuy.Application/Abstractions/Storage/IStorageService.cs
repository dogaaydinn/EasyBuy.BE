namespace EasyBuy.Application.Abstractions.Storage;

public interface IStorageService : IStorage
{
    string StrorageName { get; }
}