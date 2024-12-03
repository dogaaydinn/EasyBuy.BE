using EasyBuy.Domain.Primitives;

namespace EasyBuy.Application;

public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    T Update(T entity);
    Task HardDeleteAsync(string id);
    Task SoftDeleteAsync(string id);
    void HardDelete (T entity);
    void SoftDelete (T entity);
    Task<int> SaveChangesAsync();
    
}