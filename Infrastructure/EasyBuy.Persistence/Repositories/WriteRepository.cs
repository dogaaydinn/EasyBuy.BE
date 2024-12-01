using EasyBuy.Application.Repositories;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EasyBuy.Persistence.Repositories;

public class WriteRepository<T>(EasyBuyDbContext context) : IWriteRepository<T>
    where T : BaseEntity
{
    public DbSet<T?> DbSet => context.Set<T>();
    public async Task<bool> AddAsync(T? entity)
    {
        EntityEntry<T?> entityEntry = await DbSet.AddAsync(entity);
        return entityEntry.State == EntityState.Added;
    }

    public async Task<bool> AddRangeAsync(IEnumerable<T?> entities)
    {
        await DbSet.AddRangeAsync(entities);
        return true;
    }

    public Task<bool> UpdateAsync(T? entity)
    {
        DbSet.Update(entity);
        return Task.FromResult(true);
    }

    public bool RemoveRange(IEnumerable<T?> entities)
    {
        DbSet.RemoveRange(entities);
        return true;
    }


    public async Task<bool> RemoveAsync(string id)
    {
        T? entity = await DbSet.FindAsync(Guid.Parse(id));

        DbSet.Remove(entity);
        return true;
    }

    public async Task<int> SaveAsync()
        => await context.SaveChangesAsync();
}
