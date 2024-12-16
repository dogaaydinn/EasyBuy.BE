using EasyBuy.Application.Repositories.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Customers;

public class EfCustomerReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<AppUser>(dbContext), ICustomerReadRepository
{
}