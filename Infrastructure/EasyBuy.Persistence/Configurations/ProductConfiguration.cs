using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.PictureUrl)
            .HasMaxLength(250);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // ====================================================================
        // PERFORMANCE INDEXES
        // ====================================================================
        // Indexes for common query patterns to improve performance

        // Index on Price for range queries and sorting
        builder.HasIndex(p => p.Price)
            .HasDatabaseName("IX_Products_Price");

        // Index on CategoryId for filtering by category
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        // Composite index on CategoryId + Price for category price range queries
        builder.HasIndex(p => new { p.CategoryId, p.Price })
            .HasDatabaseName("IX_Products_CategoryId_Price");

        // Index on CreatedAt for sorting by newest/oldest
        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");

        // Index on Stock for inventory queries (out of stock, low stock)
        builder.HasIndex(p => p.Stock)
            .HasDatabaseName("IX_Products_Stock");

        // Composite index for active products sorted by creation date
        builder.HasIndex(p => new { p.IsActive, p.CreatedAt })
            .HasDatabaseName("IX_Products_IsActive_CreatedAt")
            .HasFilter("[IsActive] = 1"); // Partial index for active products only

        // Full-text search preparation: Index on Name for LIKE queries
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        // Many-to-many with ProductImageFiles commented out temporarily
        // Will be configured after TPH inheritance is properly set up
        // builder.HasMany(p => p.ProductImageFiles)
        //     .WithMany(pif => pif.Products)
        //     .UsingEntity<Dictionary<string, object>>(
        //         "ProductProductImageFile",
        //         j => j.HasOne<ProductImageFile>().WithMany().HasForeignKey("ProductImageFileId"),
        //         j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId"));
    }
}