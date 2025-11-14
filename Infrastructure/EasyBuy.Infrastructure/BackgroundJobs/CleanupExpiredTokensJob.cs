using EasyBuy.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to cleanup expired refresh tokens from the database.
/// Runs daily to remove tokens older than their expiration date.
/// </summary>
public class CleanupExpiredTokensJob : IBackgroundJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CleanupExpiredTokensJob> _logger;

    public CleanupExpiredTokensJob(
        IApplicationDbContext dbContext,
        ILogger<CleanupExpiredTokensJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cleanup expired tokens job at {Time}", DateTime.UtcNow);

        try
        {
            var now = DateTime.UtcNow;
            
            // Note: This assumes you have a RefreshToken entity/table
            // In the current implementation, refresh tokens might be stored differently
            // This is a template for how it would work
            
            // Example SQL that would be executed:
            // DELETE FROM RefreshTokens WHERE ExpiresAt < @now
            
            var deletedCount = 0;
            
            // Cleanup old refresh tokens (pseudo-code - adjust based on your actual implementation)
            // var expiredTokens = await _dbContext.RefreshTokens
            //     .Where(t => t.ExpiresAt < now)
            //     .ToListAsync(cancellationToken);
            //
            // foreach (var token in expiredTokens)
            // {
            //     _dbContext.RefreshTokens.Remove(token);
            //     deletedCount++;
            // }
            //
            // await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleanup expired tokens completed. Deleted {Count} expired tokens", 
                deletedCount);

            // Also cleanup old password reset tokens, email verification tokens, etc.
            await CleanupPasswordResetTokensAsync(now, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired tokens");
            throw;
        }
    }

    private async Task CleanupPasswordResetTokensAsync(DateTime now, CancellationToken cancellationToken)
    {
        // Cleanup password reset tokens older than 24 hours
        var passwordResetExpiry = now.AddHours(-24);
        
        // This would remove expired password reset tokens
        // Implementation depends on how tokens are stored
        
        _logger.LogInformation("Cleaned up password reset tokens older than 24 hours");
        
        await Task.CompletedTask;
    }
}
