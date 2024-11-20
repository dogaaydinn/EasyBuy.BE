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
        
        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(a => a.Street)
                .HasColumnName("StreetName")
                .IsRequired()
                .HasMaxLength(100); 
            
            a.Property(a => a.City)
                .HasColumnName("CityName")
                .IsRequired()
                .HasMaxLength(100); 
            
            a.Property(a => a.State)
                .HasColumnName("StateName")
                .IsRequired()
                .HasMaxLength(100);
            
            a.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .IsRequired()
                .HasMaxLength(20);
        });
    }
}