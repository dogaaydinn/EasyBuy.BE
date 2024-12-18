using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyBuy.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EasyBuyDbContext>
{
    public EasyBuyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EasyBuyDbContext>();
        optionsBuilder.UseNpgsql("Password=123456;Host=localhost;Port=5432;Database=EasyBuyDB;Username=postgres;");

        return new EasyBuyDbContext(optionsBuilder.Options);
    }
}