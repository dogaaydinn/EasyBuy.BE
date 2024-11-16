using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Configurations;
using EasyBuy.Persistence.ValueObjectConfigurations;
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
    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }
    public DbSet<DeliveryMethod> DeliveryMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ValueObjectConfigurations.ConfigureValueObjects(modelBuilder);
        modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
        modelBuilder.ApplyConfiguration(new AppUserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductBrandConfiguration());
        modelBuilder.ApplyConfiguration(new ProductTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new BasketConfiguration());
        modelBuilder.ApplyConfiguration(new BasketItemConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryMethodConfiguration());

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EasyBuyDbContext).Assembly);
        modelBuilder.Entity<YourMainEntity>()
            .OwnsOne(e => e.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
                price.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            });
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity<>).IsAssignableFrom(entityType.ClrType)) continue;
            var method = typeof(EasyBuyDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, [modelBuilder]);
        }

        return;

        void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => EF.Property<bool>(e, "IsDeleted") == false);
        }
    }
}