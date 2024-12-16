using EasyBuy.Application.Repositories.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Customers;

public class EfCustomerWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<AppUser>(dbContext), ICustomerWriteRepository
{
}