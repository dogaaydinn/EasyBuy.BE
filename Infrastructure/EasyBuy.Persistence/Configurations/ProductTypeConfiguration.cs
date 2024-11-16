using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                v => v,
                v => new ProductType(v).Name);
    }
}