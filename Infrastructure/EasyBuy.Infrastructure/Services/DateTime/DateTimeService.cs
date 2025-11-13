using EasyBuy.Application.Common.Interfaces;

namespace EasyBuy.Infrastructure.Services.DateTime;

public class DateTimeService : IDateTime
{
    public global::System.DateTime Now => global::System.DateTime.Now;
    public global::System.DateTime UtcNow => global::System.DateTime.UtcNow;
}
