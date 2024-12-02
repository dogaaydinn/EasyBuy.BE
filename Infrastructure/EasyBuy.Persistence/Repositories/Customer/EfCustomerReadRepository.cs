using EasyBuy.Application.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Customer;

public class EfCustomerReadRepository(EasyBuyDbContext dbContext) : EfReadRepository<AppUser>(dbContext), ICustomerReadRepository
{
    
}