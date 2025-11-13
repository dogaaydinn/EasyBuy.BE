using EasyBuy.Application.Repositories.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Customers;

public class EfCustomerWriteRepository : ICustomerWriteRepository
{
    private readonly EasyBuyDbContext _dbContext;

    public EfCustomerWriteRepository(EasyBuyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddAsync(AppUser entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(entity, cancellationToken);
        return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(AppUser entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(entity);
        return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (entity == null) return false;

        _dbContext.Users.Remove(entity);
        return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}