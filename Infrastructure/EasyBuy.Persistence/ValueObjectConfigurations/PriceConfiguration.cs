using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)") 
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3) 
            .IsRequired();
    }
}