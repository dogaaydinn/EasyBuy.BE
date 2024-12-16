using System.Linq.Expressions;
using EasyBuy.Application.Repositories;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories;

public class EfReadRepository<T>(EasyBuyDbContext context) : IReadRepository<T>
    where T : BaseEntity
{
    public DbSet<T> DbSet { get; set; } = context.Set<T>();

    public IQueryable<T?> Query => DbSet;

    public IEnumerable<T?> GetAll(Expression<Func<T, object>> orderBy = null, bool enableTracking = true)
    {
        var query = enableTracking ? Query : Query.AsNoTracking();
        return orderBy == null ? query.ToList() : query.OrderBy(orderBy).ToList();
    }

    public IQueryable<T?> Find(Expression<Func<T, bool>> predicate, bool enableTracking = true)
    {
        var query = enableTracking ? Query : Query.AsNoTracking();
        return query.Where(predicate);
    }

    public IEnumerable<T> GetWhere(Expression<Func<T?, bool>> predicate, bool enableTracking = true)
    {
        var query = DbSet.Where(predicate);
        if (!enableTracking)
            query = query.AsNoTracking();
        return query;
    }

    public async Task<T> GetSingleAsync(Expression<Func<T?, bool>> predicate, bool enableTracking = true)
    {
        var query = DbSet.AsQueryable();
        if (!enableTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<T> GetByIdAsync(string id, bool enableTracking = true)
    {
        return enableTracking
            ? await Query.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id))
            : await Query.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
    }
}