using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.Property(e => e.Value)  
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasCheckConstraint("CK_Email", "Email LIKE '%_@__%.__%'");
    }
}