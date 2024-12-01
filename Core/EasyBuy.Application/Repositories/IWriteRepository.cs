using EasyBuy.Domain.Primitives;

namespace EasyBuy.Application.Repositories;

public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<bool> AddAsync(T? entity);
    Task<bool> AddRangeAsync(IEnumerable<T?> entities);
    Task<bool> UpdateAsync(T? entity);
    bool RemoveRange(IEnumerable<T?> entities);
    Task<bool> RemoveAsync(string id);
    Task<int> SaveAsync();
    
}