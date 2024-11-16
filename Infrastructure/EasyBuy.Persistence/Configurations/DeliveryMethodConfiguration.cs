using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class DeliveryMethodConfiguration : IEntityTypeConfiguration<DeliveryMethod>
{
    public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
    {
        builder.HasKey(dm => dm.Id);

        builder.Property(dm => dm.ShortName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(dm => dm.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(dm => dm.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(dm => dm.DeliveryTime)
            .IsRequired();
    }
}