using EasyBuy.Domain.Entities.Identity;

namespace EasyBuy.Application.Repositories.Customer;

// Note: AppUser is managed by Identity's UserManager, not repositories
// This interface is kept for future extensibility but currently unused
public interface ICustomerWriteRepository
{
    Task<bool> AddAsync(AppUser entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(AppUser entity, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}