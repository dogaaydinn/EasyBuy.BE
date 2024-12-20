using System.Reflection;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using File = EasyBuy.Domain.Entities.File;

namespace EasyBuy.Persistence.Contexts;

public class EasyBuyDbContext : DbContext
{
    public EasyBuyDbContext(DbContextOptions<EasyBuyDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<Basket> Baskets { get; set; } = default!;
    public DbSet<BasketItem> BasketItems { get; set; } = default!;
    public DbSet<Delivery> Deliveries { get; set; } = default!;
    public DbSet<File> Files { get; set; } = default!;
    public DbSet<ProductImageFile> ProductImages { get; set; } = default!;
    public DbSet<InvoiceFile> Invoices { get; set; } = default!;

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

            method?.Invoke(null, [modelBuilder]);

            modelBuilder.Entity(entityType.ClrType)
                .HasKey(nameof(BaseEntity.Id));
        }

        #region Shadow Properties

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType).Property<DateTime>(CommonShadowProperties.CreatedDate);
            modelBuilder.Entity(entityType.ClrType).Property<DateTime?>(CommonShadowProperties.ModifiedDate);
            modelBuilder.Entity(entityType.ClrType).Property<DateTime?>(CommonShadowProperties.DeletedDate);
        }

        #endregion Shadow Properties
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => EF.Property<bool>(e, "IsDeleted") == false);
    }

    #region interceptors

    public override async Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(token);
        OnAfterSaveChanges();

        return result;
    }

    protected void OnBeforeSaveChanges()
    {
    }

    protected void OnAfterSaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
                entityEntry.Property(CommonShadowProperties.CreatedDate).CurrentValue = DateTime.UtcNow;

            entityEntry.Property(CommonShadowProperties.ModifiedDate).CurrentValue = DateTime.UtcNow;

            if (entityEntry.Property("IsDeleted").CurrentValue is not true)
                continue;

            entityEntry.Property(CommonShadowProperties.DeletedDate).CurrentValue = DateTime.UtcNow;
        }
    }

    #endregion
}