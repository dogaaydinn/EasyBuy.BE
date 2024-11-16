using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Total)
            .HasColumnType("decimal(18,2)") 
            .IsRequired();

        builder.Property(o => o.AppUserId)
            .IsRequired();

        builder.HasOne(o => o.AppUser)
            .WithMany()
            .HasForeignKey(o => o.AppUserId);

        builder.HasOne(o => o.DeliveryMethod)
            .WithMany()
            .HasForeignKey(o => o.DeliveryMethodId);
        
        builder.Property(o => o.OrderStatus)
            .HasConversion(
                status => status.ToString(), 
                status => (OrderStatus)Enum.Parse(typeof(OrderStatus), status)) 
            .HasMaxLength(50)
            .HasColumnName("OrderStatus");
        
        builder.OwnsOne(o => o.Sale, sale =>
        {
            sale.Property(s => s.DiscountPercentage)
                .HasColumnName("DiscountPercentage")
                .IsRequired();

            sale.Property(s => s.StartDate)
                .HasColumnName("StartDate")
                .IsRequired();

            sale.Property(s => s.EndDate)
                .HasColumnName("EndDate")
                .IsRequired();
        });
    }
}