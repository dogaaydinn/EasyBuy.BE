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

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

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

        builder.HasMany(p => p.Orders)
            .WithMany(o => o.OrderItems.Select(oi => oi.Product))
            .UsingEntity(j => j.ToTable("ProductOrders"));
        
        builder.OwnsOne(p => p.Quantity, quantity =>
        {
            quantity.Property(q => q.Value)
                .HasColumnName("Quantity")
                .IsRequired()
                .HasColumnType("int");
        });

        builder.OwnsOne(p => p.Description, description =>
        {
            description.Property(d => d.Value)
                .HasColumnName("ProductDescription")  
                .IsRequired()  
                .HasMaxLength(500);  
        });
        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("ProductName")  
                .IsRequired() 
                .HasMaxLength(100); 
        });
    }
}