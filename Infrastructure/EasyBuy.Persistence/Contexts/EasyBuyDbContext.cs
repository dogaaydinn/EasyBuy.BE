using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Contexts;

public class EasyBuyDbContext : DbContext
{
    public EasyBuyDbContext(DbContextOptions<EasyBuyDbContext> options) : base(options)
    {
    }
    
}