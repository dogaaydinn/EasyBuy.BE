using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class ProductNameConfiguration : IEntityTypeConfiguration<ProductName>
{
    public void Configure(EntityTypeBuilder<ProductName> builder)
    {
        builder.Property(p => p.Value)
            .HasColumnName("ProductName") 
            .IsRequired()  
            .HasMaxLength(100); 
    }
}