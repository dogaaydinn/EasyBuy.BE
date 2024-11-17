using EasyBuy.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Address)
                .HasColumnName("Email") 
                .IsRequired()
                .HasMaxLength(150);
        });
        
        builder.HasIndex(u => u.Email.Address).HasDatabaseName("IX_AppUser_Email");
        
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Number)
                .HasColumnName("PhoneNumber") 
                .IsRequired()
                .HasMaxLength(20); 
        });
        
        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.PostalCode.Code)
                .HasColumnName("PostalCode")
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}