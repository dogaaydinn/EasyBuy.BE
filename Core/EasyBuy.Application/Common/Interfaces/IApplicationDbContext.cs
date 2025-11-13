using EasyBuy.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
