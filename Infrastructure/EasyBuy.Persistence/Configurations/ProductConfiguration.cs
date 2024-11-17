using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.PictureUrl)
            .HasMaxLength(250);

        builder.HasOne(p => p.ProductType)
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductBrand)
            .WithMany()
            .HasForeignKey(p => p.ProductBrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ProductOrders)
            .WithOne()
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(p => p.Quantity, quantity =>
        {
            quantity.Property(q => q.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        builder.OwnsOne(p => p.Description, description =>
        {
            description.Property(d => d.Description)
                .HasColumnName("ProductDescription")
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.Property(p => p.Price)
            .HasColumnName("Price")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.OwnsOne(p => p.Sale, s =>
        {
            s.Property(s => s.DiscountPercentage)
                .HasColumnName("DiscountPercentage")
                .IsRequired();

            s.Property(s => s.StartDate)
                .HasColumnName("StartDate")
                .IsRequired();

            s.Property(s => s.EndDate)
                .HasColumnName("EndDate")
                .IsRequired();
        });
    }
}