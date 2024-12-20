using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductImageFileConfiguration : IEntityTypeConfiguration<ProductImageFile>
{
    public void Configure(EntityTypeBuilder<ProductImageFile> builder)
    {
        builder.HasMany(pif => pif.Products)
            .WithMany(p => p.ProductImageFiles)
            .UsingEntity<Dictionary<string, object>>(
                "ProductProductImageFile",
                j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId"),
                j => j.HasOne<ProductImageFile>().WithMany().HasForeignKey("ProductImageFileId"));
    }
}