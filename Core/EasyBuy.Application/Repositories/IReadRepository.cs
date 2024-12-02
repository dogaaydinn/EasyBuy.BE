using System.Linq.Expressions;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Application;

public interface IReadRepository<T> : IRepository<T> where T : BaseEntity
{
    IQueryable<T?> Query { get; }
    IEnumerable<T?> GetAll();
    IEnumerable<T?> Find(Expression<Func<T?, bool>> predicate);
    IEnumerable<T?> GetWhere(Expression<Func<T?, bool>> predicate);
    Task<T?> GetSingleAsync(Expression<Func<T?, bool>> predicate);
    Task<T> GetByIdAsync(string id);
    
    
}
//düzenlenmeli