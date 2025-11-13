using EasyBuy.Application.Repositories.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories.Customers;

public class EfCustomerReadRepository : ICustomerReadRepository
{
    private readonly EasyBuyDbContext _dbContext;

    public EfCustomerReadRepository(EasyBuyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync([id], cancellationToken);
    }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.ToListAsync(cancellationToken);
    }
}