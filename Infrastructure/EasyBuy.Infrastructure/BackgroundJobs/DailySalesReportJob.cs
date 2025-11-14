using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Repositories.Order;
using EasyBuy.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to generate and send daily sales reports.
/// Runs daily at midnight to summarize previous day's sales.
/// </summary>
public class DailySalesReportJob : IBackgroundJob
{
    private readonly IOrderReadRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailySalesReportJob> _logger;

    public DailySalesReportJob(
        IOrderReadRepository orderRepository,
        IEmailService emailService,
        ILogger<DailySalesReportJob> logger)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting daily sales report job at {Time}", DateTime.UtcNow);

        try
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var today = DateTime.UtcNow.Date;

            // Get all orders from yesterday
            var allOrders = await _orderRepository.GetAllAsync();
            var yesterdayOrders = allOrders
                .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today)
                .ToList();

            // Calculate statistics
            var totalOrders = yesterdayOrders.Count;
            var completedOrders = yesterdayOrders.Count(o => o.Status == OrderStatus.Delivered);
            var cancelledOrders = yesterdayOrders.Count(o => o.Status == OrderStatus.Cancelled);
            var totalRevenue = yesterdayOrders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount);
            var averageOrderValue = completedOrders > 0 ? totalRevenue / completedOrders : 0;

            var report = new SalesReport
            {
                Date = yesterday,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                AverageOrderValue = averageOrderValue
            };

            // Send email report to admins
            await SendReportEmailAsync(report);

            _logger.LogInformation(
                "Daily sales report generated: Date={Date}, Orders={Orders}, Revenue={Revenue:C}",
                yesterday, totalOrders, totalRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales report");
            throw;
        }
    }

    private async Task SendReportEmailAsync(SalesReport report)
    {
        var emailBody = GenerateReportEmailBody(report);

        // In production, send to multiple admin emails
        await _emailService.SendEmailAsync(
            to: "admin@easybuy.com", // Would fetch from configuration
            subject: $"Daily Sales Report - {report.Date:yyyy-MM-dd}",
            body: emailBody);

        _logger.LogInformation("Sent daily sales report email for {Date}", report.Date);
    }

    private static string GenerateReportEmailBody(SalesReport report)
    {
        var conversionRate = report.TotalOrders > 0 
            ? (double)report.CompletedOrders / report.TotalOrders * 100 
            : 0;

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif; }}
                    .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                    .metrics {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }}
                    .metric {{ background-color: #f5f5f5; padding: 15px; border-radius: 4px; }}
                    .metric-value {{ font-size: 24px; font-weight: bold; color: #2196F3; }}
                    .metric-label {{ color: #666; font-size: 12px; margin-top: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Daily Sales Report</h1>
                        <p>{report.Date:MMMM dd, yyyy}</p>
                    </div>
                    <div class='metrics'>
                        <div class='metric'>
                            <div class='metric-value'>{report.TotalOrders}</div>
                            <div class='metric-label'>Total Orders</div>
                        </div>
                        <div class='metric'>
                            <div class='metric-value'>{report.CompletedOrders}</div>
                            <div class='metric-label'>Completed Orders</div>
                        </div>
                        <div class='metric'>
                            <div class='metric-value'>${report.TotalRevenue:N2}</div>
                            <div class='metric-label'>Total Revenue</div>
                        </div>
                        <div class='metric'>
                            <div class='metric-value'>${report.AverageOrderValue:N2}</div>
                            <div class='metric-label'>Average Order Value</div>
                        </div>
                        <div class='metric'>
                            <div class='metric-value'>{report.CancelledOrders}</div>
                            <div class='metric-label'>Cancelled Orders</div>
                        </div>
                        <div class='metric'>
                            <div class='metric-value'>{conversionRate:F1}%</div>
                            <div class='metric-label'>Completion Rate</div>
                        </div>
                    </div>
                    <p style='color: #666; font-size: 12px; margin-top: 20px;'>
                        This is an automated daily sales report generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.
                    </p>
                </div>
            </body>
            </html>";
    }

    private class SalesReport
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
