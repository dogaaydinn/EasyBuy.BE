using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations
{
    public class BasketConfiguration : IEntityTypeConfiguration<Basket>
    {
        public void Configure(EntityTypeBuilder<Basket> builder)
        {
            builder.HasKey(b => b.Id);

            // Configure the relationship between Basket and AppUser
            builder.HasOne(b => b.AppUser)
                .WithOne() // One Basket per AppUser
                .HasForeignKey<Basket>(b => b.AppUserId) // Foreign key in Basket
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete on AppUser deletion

            // Configure the relationship between Basket and BasketItems
            builder.HasMany(b => b.Items)
                .WithOne(bi => bi.Basket) // Each BasketItem belongs to one Basket
                .HasForeignKey(bi => bi.BasketId) // Foreign key in BasketItem
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete on Basket deletion
        }
    }
}