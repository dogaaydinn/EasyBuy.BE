using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductItemOrderedConfiguration : IEntityTypeConfiguration<ProductItemOrdered>
{
    public void Configure(EntityTypeBuilder<ProductItemOrdered> builder)
    {
        builder.HasKey(pio => pio.ProductId);

        builder.Property(pio => pio.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(pio => pio.ProductPrice, pp =>
        {
            pp.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });
    }
}