using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Repositories.Product;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to synchronize inventory and send low stock alerts.
/// Runs every 6 hours to check stock levels and notify administrators.
/// </summary>
public class InventorySynchronizationJob : IBackgroundJob
{
    private readonly IProductReadRepository _productRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<InventorySynchronizationJob> _logger;
    
    private const int LowStockThreshold = 10;
    private const int CriticalStockThreshold = 3;

    public InventorySynchronizationJob(
        IProductReadRepository productRepository,
        IEmailService emailService,
        ILogger<InventorySynchronizationJob> logger)
    {
        _productRepository = productRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting inventory synchronization job at {Time}", DateTime.UtcNow);

        try
        {
            // Get all products
            var allProducts = await _productRepository.GetAllAsync();
            var activeProducts = allProducts.Where(p => p.IsActive).ToList();

            // Categorize products by stock level
            var outOfStock = activeProducts.Where(p => p.Quantity == 0).ToList();
            var criticalStock = activeProducts.Where(p => p.Quantity > 0 && p.Quantity <= CriticalStockThreshold).ToList();
            var lowStock = activeProducts.Where(p => p.Quantity > CriticalStockThreshold && p.Quantity <= LowStockThreshold).ToList();

            _logger.LogInformation(
                "Inventory check: Total={Total}, OutOfStock={OutOfStock}, Critical={Critical}, Low={Low}",
                activeProducts.Count, outOfStock.Count, criticalStock.Count, lowStock.Count);

            // Send alerts if there are inventory issues
            if (outOfStock.Any() || criticalStock.Any() || lowStock.Any())
            {
                await SendInventoryAlertEmailAsync(outOfStock, criticalStock, lowStock);
            }

            // Log individual critical items
            foreach (var product in criticalStock)
            {
                _logger.LogWarning(
                    "CRITICAL STOCK: Product '{ProductName}' (ID: {ProductId}) has only {Quantity} unit(s) remaining",
                    product.Name, product.Id, product.Quantity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during inventory synchronization");
            throw;
        }
    }

    private async Task SendInventoryAlertEmailAsync(
        List<Domain.Entities.Product> outOfStock,
        List<Domain.Entities.Product> criticalStock,
        List<Domain.Entities.Product> lowStock)
    {
        var emailBody = GenerateInventoryAlertEmailBody(outOfStock, criticalStock, lowStock);

        await _emailService.SendEmailAsync(
            to: "inventory@easybuy.com", // Would fetch from configuration
            subject: $"Inventory Alert - {DateTime.UtcNow:yyyy-MM-dd}",
            body: emailBody);

        _logger.LogInformation("Sent inventory alert email");
    }

    private static string GenerateInventoryAlertEmailBody(
        List<Domain.Entities.Product> outOfStock,
        List<Domain.Entities.Product> criticalStock,
        List<Domain.Entities.Product> lowStock)
    {
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container { max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif; }
                    .header { background-color: #FF5722; color: white; padding: 20px; text-align: center; }
                    .alert-section { margin: 20px 0; }
                    .alert-title { font-weight: bold; padding: 10px; background-color: #f5f5f5; margin-bottom: 10px; }
                    .critical { border-left: 4px solid #F44336; padding-left: 10px; }
                    .warning { border-left: 4px solid #FF9800; padding-left: 10px; }
                    .info { border-left: 4px solid #2196F3; padding-left: 10px; }
                    .product-item { padding: 8px; margin: 5px 0; background-color: #fafafa; }
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⚠️ Inventory Alert</h1>
                        <p>" + DateTime.UtcNow.ToString("MMMM dd, yyyy HH:mm UTC") + @"</p>
                    </div>";

        if (outOfStock.Any())
        {
            html += @"
                    <div class='alert-section critical'>
                        <div class='alert-title'>🚫 OUT OF STOCK (" + outOfStock.Count + @" items)</div>";
            foreach (var product in outOfStock.Take(10))
            {
                html += $@"
                        <div class='product-item'>{product.Name} (SKU: {product.Id})</div>";
            }
            if (outOfStock.Count > 10)
            {
                html += $@"
                        <div class='product-item'>... and {outOfStock.Count - 10} more items</div>";
            }
            html += "</div>";
        }

        if (criticalStock.Any())
        {
            html += @"
                    <div class='alert-section warning'>
                        <div class='alert-title'>⚠️ CRITICAL STOCK (" + criticalStock.Count + @" items - 1-3 units remaining)</div>";
            foreach (var product in criticalStock.Take(10))
            {
                html += $@"
                        <div class='product-item'>{product.Name} - <strong>{product.Quantity} unit(s)</strong> remaining</div>";
            }
            if (criticalStock.Count > 10)
            {
                html += $@"
                        <div class='product-item'>... and {criticalStock.Count - 10} more items</div>";
            }
            html += "</div>";
        }

        if (lowStock.Any())
        {
            html += @"
                    <div class='alert-section info'>
                        <div class='alert-title'>ℹ️ LOW STOCK (" + lowStock.Count + @" items - 4-10 units remaining)</div>";
            foreach (var product in lowStock.Take(5))
            {
                html += $@"
                        <div class='product-item'>{product.Name} - {product.Quantity} unit(s) remaining</div>";
            }
            if (lowStock.Count > 5)
            {
                html += $@"
                        <div class='product-item'>... and {lowStock.Count - 5} more items</div>";
            }
            html += "</div>";
        }

        html += @"
                    <p style='margin-top: 30px; color: #666; font-size: 12px;'>
                        This is an automated inventory alert. Please review and restock items as necessary.
                    </p>
                </div>
            </body>
            </html>";

        return html;
    }
}
