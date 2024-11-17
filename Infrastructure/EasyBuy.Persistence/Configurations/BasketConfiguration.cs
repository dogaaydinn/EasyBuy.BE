using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.HasOne(b => b.AppUser)
            .WithOne() 
            .HasForeignKey<Basket>(b => b.AppUserId) 
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasMany(b => b.Items)
            .WithOne(bi => bi.Basket) 
            .HasForeignKey(bi => bi.BasketId) 
            .OnDelete(DeleteBehavior.Cascade); 
    }
}