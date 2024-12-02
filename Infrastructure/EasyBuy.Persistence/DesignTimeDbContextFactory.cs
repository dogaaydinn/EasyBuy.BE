using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyBuy.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EasyBuyDbContext>
{
    public EasyBuyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EasyBuyDbContext>();
        optionsBuilder.UseNpgsql("Password=*******;Host=localhost;Port=5432;Database=postgres;");

        return new EasyBuyDbContext(optionsBuilder.Options);
    }
}