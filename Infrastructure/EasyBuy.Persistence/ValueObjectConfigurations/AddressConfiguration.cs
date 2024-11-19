using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.Street)
            .HasColumnName("StreetName")
            .IsRequired()
            .HasMaxLength(100); 
        
        builder.Property(a => a.City)
            .HasColumnName("CityName")
            .IsRequired()
            .HasMaxLength(100); 
        
        builder.Property(a => a.State)
            .HasColumnName("StateName")
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.PostalCode)
            .HasColumnName("PostalCode")
            .IsRequired()
            .HasMaxLength(20);
    }
}