using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class ProductDescriptionConfiguration : IEntityTypeConfiguration<ProductDescription>
{
    public void Configure(EntityTypeBuilder<ProductDescription> builder)
    {
        builder.Property(p => p.Value)
            .HasColumnName("ProductDescription")  
            .IsRequired()  
            .HasMaxLength(500);  
    }
}