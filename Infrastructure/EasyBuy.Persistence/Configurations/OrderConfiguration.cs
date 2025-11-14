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

        // ====================================================================
        // PERFORMANCE INDEXES
        // ====================================================================
        // Indexes for common order query patterns

        // Index on UserId for user's order history queries
        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("IX_Orders_UserId");

        // Index on OrderStatus for filtering by status (pending, completed, etc.)
        builder.HasIndex(o => o.OrderStatus)
            .HasDatabaseName("IX_Orders_OrderStatus");

        // Composite index on UserId + OrderStatus for user orders filtered by status
        builder.HasIndex(o => new { o.UserId, o.OrderStatus })
            .HasDatabaseName("IX_Orders_UserId_OrderStatus");

        // Index on OrderDate for date range queries and sorting
        builder.HasIndex(o => o.OrderDate)
            .HasDatabaseName("IX_Orders_OrderDate");

        // Composite index on OrderStatus + OrderDate for status + time filtering
        builder.HasIndex(o => new { o.OrderStatus, o.OrderDate })
            .HasDatabaseName("IX_Orders_OrderStatus_OrderDate");

        // Index on CreatedAt for audit and sorting purposes
        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        // Index on OrderNumber for lookups by order number
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");
    }
}