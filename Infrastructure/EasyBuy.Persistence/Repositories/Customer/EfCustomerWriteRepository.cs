using EasyBuy.Application.Customer;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Customer;

public class EfCustomerWriteRepository(EasyBuyDbContext dbContext) : EfWriteRepository<AppUser>(dbContext), ICustomerWriteRepository
{
    
}