using System.Reflection;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Contexts;

public class EasyBuyDbContext : DbContext
{
    public EasyBuyDbContext(DbContextOptions<EasyBuyDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EasyBuyDbContext).Assembly);
        
        var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;
            
            var method = typeof(EasyBuyDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, new object[] { modelBuilder });
            
            modelBuilder.Entity(entityType.ClrType)
                .HasKey(nameof(BaseEntity.Id));
        }
    }


    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => EF.Property<bool>(e, "IsDeleted") == false);
    }
}