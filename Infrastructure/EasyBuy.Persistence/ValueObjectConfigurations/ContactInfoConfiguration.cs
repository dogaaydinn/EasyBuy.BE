using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class ContactInfoConfiguration : IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder.OwnsOne<PhoneNumber>(c => c.PhoneNumber, pn =>
        {
            pn.Property(p => p.Number)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(15);
        });


        builder.OwnsOne(c => c.Email, e =>
        {
            e.Property<string>(em => em.Address)
                .HasColumnName("EmailAddress")
                .IsRequired()
                .HasMaxLength(150);
            e.HasIndex(em => em.Address); 
        });
    }
}