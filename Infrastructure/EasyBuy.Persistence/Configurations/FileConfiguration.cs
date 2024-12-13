using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = EasyBuy.Domain.Entities.File;

namespace EasyBuy.Persistence.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
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
    }
}