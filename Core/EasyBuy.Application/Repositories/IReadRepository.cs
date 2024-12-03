using System.Linq.Expressions;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Application;

public interface IReadRepository<T> : IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query { get; }
    IEnumerable<T> GetAll(Expression<Func<T, object>> orderBy = null, bool enableTracking = true);
    IQueryable<T> Find(Expression<Func<T, bool>> predicate, bool enableTracking = true);
    IEnumerable<T> GetWhere(Expression<Func<T?, bool>> predicate, bool enableTracking = true);
    Task<T> GetSingleAsync(Expression<Func<T?, bool>> predicate, bool enableTracking = true);
    Task<T> GetByIdAsync(string id, bool enableTracking = true);

}