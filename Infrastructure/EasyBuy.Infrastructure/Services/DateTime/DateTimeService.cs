using EasyBuy.Application.Common.Interfaces;

namespace EasyBuy.Infrastructure.Services.DateTime;

public class DateTimeService : IDateTime
{
    public System.DateTime Now => System.DateTime.Now;
    public System.DateTime UtcNow => System.DateTime.UtcNow;
}
