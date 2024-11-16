using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.Property(s => s.DiscountPercentage)
            .HasColumnName("DiscountPercentage")
            .IsRequired();

        builder.Property(s => s.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        builder.Property(s => s.EndDate)
            .HasColumnName("EndDate")
            .IsRequired();
    }
}