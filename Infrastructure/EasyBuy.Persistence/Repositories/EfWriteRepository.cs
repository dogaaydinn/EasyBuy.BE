using EasyBuy.Application;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories;

public class EfWriteRepository<T>(EasyBuyDbContext context) : IWriteRepository<T>
    where T : BaseEntity
{
    
    public DbSet<T> DbSet { get; set; } = context.Set<T>();
    
    public async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
        return entities;
    }

    public T Update(T entity)
    {
        DbSet.Update(entity);
        return entity;
    }

    public async Task HardDeleteAsync(string id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity != null)
            return;
        
        DbSet.Remove(entity);
    }
    
    public async Task SoftDeleteAsync(string id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity == null)
            return;
        
        entity.MarkAsDeleted();
        DbSet.Update(entity);
    }
    
    public void HardDelete(T entity)
    {
        DbSet.Remove(entity);
    }
    
    public void SoftDelete(T entity)
    {
        entity.MarkAsDeleted();
        DbSet.Update(entity);
    }

    public async Task<int> SaveChangesAsync()
        => await context.SaveChangesAsync();
    
}
