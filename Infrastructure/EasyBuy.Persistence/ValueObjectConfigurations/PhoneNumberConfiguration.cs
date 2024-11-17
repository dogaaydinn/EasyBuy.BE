using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.Property(p => p.CountryCode)
            .HasColumnName("CountryCode")
            .IsRequired() 
            .HasMaxLength(5); 

        builder.Property(p => p.Number)
            .HasColumnName("PhoneNumber")
            .IsRequired() 
            .HasMaxLength(15); 
    }
}