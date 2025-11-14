using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.Property(r => r.Title)
            .HasMaxLength(200);

        // Relationship with Product
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================================
        // PERFORMANCE INDEXES
        // ====================================================================
        // Indexes for common review query patterns

        // Index on ProductId for product reviews queries
        builder.HasIndex(r => r.ProductId)
            .HasDatabaseName("IX_Reviews_ProductId");

        // Index on UserId for user's reviews
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Reviews_UserId");

        // Composite index on ProductId + Rating for filtered product reviews
        builder.HasIndex(r => new { r.ProductId, r.Rating })
            .HasDatabaseName("IX_Reviews_ProductId_Rating");

        // Index on Rating for filtering by star rating
        builder.HasIndex(r => r.Rating)
            .HasDatabaseName("IX_Reviews_Rating");

        // Index on CreatedAt for sorting by newest reviews
        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_Reviews_CreatedAt");

        // Composite index for product reviews sorted by date
        builder.HasIndex(r => new { r.ProductId, r.CreatedAt })
            .HasDatabaseName("IX_Reviews_ProductId_CreatedAt");
    }
}
