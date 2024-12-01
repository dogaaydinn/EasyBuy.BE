using System.Linq.Expressions;
using EasyBuy.Application.Repositories;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories;

public class ReadRepository<T>(EasyBuyDbContext context) : IReadRepository<T>
    where T : BaseEntity
{
    public DbSet<T> DbSet => context.Set<T>();
    public IQueryable<T?> Query => DbSet;

    public IEnumerable<T?> GetAll()
=> Query.ToList();
    
    public IEnumerable<T?> Find(Expression<Func<T?, bool>> predicate)
    => Query.Where(predicate).ToList();
    
    public async Task<T> GetByIdAsync(string id)
    => await Query.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id.ToString()));
    

    public IEnumerable<T?> GetWhere(Expression<Func<T?, bool>> predicate)
 => Query.Where(predicate).ToList();

    public Task<T?> GetSingleAsync(Expression<Func<T?, bool>> predicate)
    => Query.SingleOrDefaultAsync(predicate);
}