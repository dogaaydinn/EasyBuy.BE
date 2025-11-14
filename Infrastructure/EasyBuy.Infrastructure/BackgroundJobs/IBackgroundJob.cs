namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Base interface for all background jobs.
/// Provides common Execute method for Hangfire integration.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Execute the background job.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
