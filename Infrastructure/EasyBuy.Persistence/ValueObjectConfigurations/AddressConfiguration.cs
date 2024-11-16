using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Address = EasyBuy.Domain.Primitives.Address;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.OwnsOne(a => a.Street, sa =>
        {
            sa.Property<string>(s => s.Name).HasColumnName("StreetName").IsRequired();
            sa.Property<int>(s => s.Number).HasColumnName("StreetNumber").IsRequired();
        });

        builder.OwnsOne(a => a.City, sa =>
        {
            sa.Property<string>(s => s.Name).HasColumnName("CityName").IsRequired();
        });
            
        builder.Property(a => a.ZipCode).HasColumnName("ZipCode").IsRequired();
        builder.Property(a => a.PostalCode).HasColumnName("PostalCode").IsRequired();
    }
}