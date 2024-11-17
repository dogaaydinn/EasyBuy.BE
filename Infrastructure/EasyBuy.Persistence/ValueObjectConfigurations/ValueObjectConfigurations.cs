using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public static class ValueObjectConfigurations
{
    public static void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new ContactInfoConfiguration());
        modelBuilder.ApplyConfiguration(new PriceConfiguration());
        modelBuilder.ApplyConfiguration(new EmailConfiguration());
        modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
        modelBuilder.ApplyConfiguration(new SaleConfiguration());
        modelBuilder.ApplyConfiguration(new ProductNameConfiguration());
        modelBuilder.ApplyConfiguration(new QuantityConfiguration());
        modelBuilder.ApplyConfiguration(new PostalCodeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductDescriptionConfiguration());
            
            
        modelBuilder.Owned<Price>();
        modelBuilder.Owned<Email>();
        modelBuilder.Owned<PhoneNumber>();
        modelBuilder.Owned<Sale>();
        modelBuilder.Owned<ProductName>();
        modelBuilder.Owned<Quantity>();
        modelBuilder.Owned<PostalCode>();
        modelBuilder.Owned<ProductDescription>();
            
    }
}