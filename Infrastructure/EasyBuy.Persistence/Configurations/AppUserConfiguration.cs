using EasyBuy.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Configuring FirstName and LastName as required properties with max length
            builder.Property(u => u.FirstName)
                .IsRequired() // Ensures that FirstName is required
                .HasMaxLength(50); // Limits the length to 50 characters

            builder.Property(u => u.LastName)
                .IsRequired() // Ensures that LastName is required
                .HasMaxLength(50); // Limits the length to 50 characters

            // Configuring the Email value object
            builder.OwnsOne(
                u => u.Email, 
                email =>
                {
                    email.Property(e => e.Value)
                        .IsRequired() // Ensures that the email value is required
                        .HasMaxLength(150) // Limits the email length to 150 characters
                        .HasColumnName("Email"); // Column name in the database
                });

            // Configuring the PhoneNumber value object (if needed)
            builder.OwnsOne(
                u => u.PhoneNumber, 
                phone =>
                {
                    phone.Property(p => p.Value)
                        .HasMaxLength(20) // Adjust max length for phone number as per your needs
                        .HasColumnName("PhoneNumber"); // Column name in the database
                });

            // Configuring the Address value object
            builder.OwnsOne(
                u => u.Address, 
                address =>
                {
                    address.Property(a => a.Street)
                        .HasMaxLength(100) // Adjust the length as per your needs
                        .HasColumnName("Street"); // Column name in the database

                    address.Property(a => a.City)
                        .HasMaxLength(50) // Adjust the length as per your needs
                        .HasColumnName("City"); // Column name in the database

                    address.Property(a => a.ZipCode)
                        .HasMaxLength(20) // Adjust the length as per your needs
                        .HasColumnName("ZipCode"); // Column name in the database
                });
        }
    }
}
