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

        builder.Property(o => o.OrderStatus)
            .HasConversion(
                status => status.ToString(),
                status => (OrderStatus)Enum.Parse(typeof(OrderStatus), status))
            .HasMaxLength(50)
            .HasColumnName("OrderStatus");

        builder.HasOne(o => o.Delivery)
            .WithMany()
            .HasForeignKey(o => o.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}