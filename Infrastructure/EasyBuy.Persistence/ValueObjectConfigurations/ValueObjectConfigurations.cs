using Microsoft.EntityFrameworkCore;
using Address = EasyBuy.Domain.Primitives.Address;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public static class ValueObjectConfigurations
{
    public static void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration<Address>(new AddressConfiguration());
        modelBuilder.ApplyConfiguration<ContactInfo>(new ContactInfoConfiguration());

    }
}