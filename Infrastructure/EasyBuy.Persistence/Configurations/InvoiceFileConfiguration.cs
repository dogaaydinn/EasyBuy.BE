using EasyBuy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.Configurations;

public class InvoiceFileConfiguration : IEntityTypeConfiguration<InvoiceFile>
{
    public void Configure(EntityTypeBuilder<InvoiceFile> builder)
    {
        builder.Property(x => x.Price)
            .IsRequired();
        
    }
}