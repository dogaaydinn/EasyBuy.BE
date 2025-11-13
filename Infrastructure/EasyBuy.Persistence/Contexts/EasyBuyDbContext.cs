using System.Reflection;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;
using EasyBuy.Persistence.Configurations;
using EasyBuy.Persistence.Constants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using File = EasyBuy.Domain.Entities.File;

namespace EasyBuy.Persistence.Contexts;

public class EasyBuyDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext
{
    public EasyBuyDbContext(DbContextOptions<EasyBuyDbContext> options)
        : base(options)
    {
    }

    // Identity (Users, Roles handled by IdentityDbContext)
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    // Products & Catalog
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    // ProductImageFile is accessed via Files DbSet (TPH inheritance)

    // Orders & Shopping
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<Basket> Baskets { get; set; } = default!;
    public DbSet<BasketItem> BasketItems { get; set; } = default!;
    public DbSet<Delivery> Deliveries { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;

    // Customer Data
    public DbSet<Address> Addresses { get; set; } = default!;
    public DbSet<Wishlist> Wishlists { get; set; } = default!;
    public DbSet<WishlistItem> WishlistItems { get; set; } = default!;

    // Promotions
    public DbSet<Coupon> Coupons { get; set; } = default!;

    // Files (InvoiceFile is a derived type and will be included automatically via TPH inheritance)
    public DbSet<File> Files { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EasyBuyDbContext).Assembly);

        // Configure Identity tables with custom names
        modelBuilder.Entity<AppUser>(b =>
        {
            b.ToTable("Users");
            b.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<AppRole>(b =>
        {
            b.ToTable("Roles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(rt => rt.Id);
            b.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(rt => rt.Token).IsUnique();
        });

        var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            // Only configure key and filters for root types (not derived types in TPH hierarchy)
            // Derived types inherit the key configuration and filters from their root type
            var baseType = entityType.ClrType.BaseType;
            var isRootEntity = baseType == typeof(BaseEntity) || !typeof(BaseEntity).IsAssignableFrom(baseType);

            if (isRootEntity)
            {
                // Apply soft delete filter
                var method = typeof(EasyBuyDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, [modelBuilder]);

                // Configure key
                modelBuilder.Entity(entityType.ClrType)
                    .HasKey(nameof(BaseEntity.Id));
            }
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