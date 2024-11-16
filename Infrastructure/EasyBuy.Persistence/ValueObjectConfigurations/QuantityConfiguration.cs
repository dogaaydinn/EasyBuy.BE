using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class QuantityConfiguration : IEntityTypeConfiguration<Quantity>
{
    public void Configure(EntityTypeBuilder<Quantity> builder)
    {
        builder.Property(q => q.Value)
            .HasColumnName("Quantity") 
            .IsRequired()              
            .HasColumnType("int");     
    }
}