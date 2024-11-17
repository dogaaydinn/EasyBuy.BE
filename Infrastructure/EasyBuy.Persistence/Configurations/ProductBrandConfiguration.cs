using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductBrandConfiguration : IEntityTypeConfiguration<ProductBrand>
{
    public void Configure(EntityTypeBuilder<ProductBrand> builder)
    {
        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                v => v,
                v => new ProductBrand(v, Guid.NewGuid()).Name);
    }
}