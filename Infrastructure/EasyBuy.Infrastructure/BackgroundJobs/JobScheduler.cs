using Hangfire;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Configures and schedules all recurring background jobs using Hangfire.
/// </summary>
public class JobScheduler
{
    private readonly ILogger<JobScheduler> _logger;

    public JobScheduler(ILogger<JobScheduler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Schedule all recurring jobs.
    /// Call this method during application startup.
    /// </summary>
    public void ScheduleRecurringJobs()
    {
        _logger.LogInformation("Scheduling recurring background jobs");

        // Abandoned Cart Reminder - Every 2 hours
        RecurringJob.AddOrUpdate<AbandonedCartReminderJob>(
            "abandoned-cart-reminder",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 */2 * * *"); // Cron: At minute 0 past every 2nd hour

        // Daily Sales Report - Daily at midnight UTC
        RecurringJob.AddOrUpdate<DailySalesReportJob>(
            "daily-sales-report",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily(0, 0)); // Cron: At 00:00

        // Cleanup Expired Tokens - Daily at 3 AM UTC
        RecurringJob.AddOrUpdate<CleanupExpiredTokensJob>(
            "cleanup-expired-tokens",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily(3, 0)); // Cron: At 03:00

        // Inventory Synchronization - Every 6 hours
        RecurringJob.AddOrUpdate<InventorySynchronizationJob>(
            "inventory-synchronization",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 */6 * * *"); // Cron: At minute 0 past every 6th hour

        // Email Queue Processor - Every 5 minutes
        RecurringJob.AddOrUpdate<EmailQueueProcessorJob>(
            "email-queue-processor",
            job => job.ExecuteAsync(CancellationToken.None),
            "*/5 * * * *"); // Cron: Every 5 minutes

        _logger.LogInformation("All recurring background jobs scheduled successfully");
    }
}
