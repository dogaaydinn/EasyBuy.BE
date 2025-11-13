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