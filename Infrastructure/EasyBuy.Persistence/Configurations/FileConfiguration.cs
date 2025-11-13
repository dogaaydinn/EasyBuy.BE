using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = EasyBuy.Domain.Entities.File;

namespace EasyBuy.Persistence.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        // Key configuration removed - let EF Core conventions handle it via BaseEntity.Id
        // Explicit HasKey was causing TPH inheritance issues

        // Configure TPH inheritance with a discriminator
        builder.HasDiscriminator<string>("FileType")
            .HasValue<File>("File")
            .HasValue<InvoiceFile>("InvoiceFile")
            .HasValue<ProductImageFile>("ProductImageFile");

        builder.Property(f => f.Name)
            .IsRequired();

        builder.Property(f => f.Path)
            .IsRequired();

        builder.Property(f => f.Extension)
            .HasMaxLength(10);

        builder.Property(f => f.Size)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .IsRequired();

        // Configure InvoiceFile specific properties (shadow property for TPH)
        builder.Property<decimal?>("Price")
            .HasColumnName("Price");
    }
}