using EasyBuy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Address = EasyBuy.Domain.Primitives.Address;

namespace EasyBuy.Persistence.ValueObjectConfigurations;

public static class ValueObjectConfiguration
{
    public static void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Owned<Price>();
        modelBuilder.Owned<Email>();
        modelBuilder.Owned<PhoneNumber>();
        modelBuilder.Owned<Address>();
    }
}