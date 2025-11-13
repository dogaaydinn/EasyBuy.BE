using EasyBuy.Domain.Entities.Identity;

namespace EasyBuy.Application.Repositories.Customer;

// Note: AppUser is managed by Identity's UserManager, not repositories
// This interface is kept for future extensibility but currently unused
public interface ICustomerReadRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);
}