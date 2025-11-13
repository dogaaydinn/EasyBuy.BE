using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Persistence.Data;

/// <summary>
/// Seeds the database with initial data for development and testing
/// </summary>
public class DatabaseSeeder
{
    private readonly EasyBuyDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        EasyBuyDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all initial data
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Ensure database is created and migrations are applied
            await _context.Database.MigrateAsync();

            // Seed in order due to foreign key dependencies
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();
            await SeedCouponsAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    /// <summary>
    /// Seeds default roles
    /// </summary>
    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        var roles = new[]
        {
            new AppRole("Admin") { Description = "System administrator with full access" },
            new AppRole("Manager") { Description = "Store manager with elevated privileges" },
            new AppRole("Customer") { Description = "Regular customer user" },
            new AppRole("Vendor") { Description = "Product vendor/seller" }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", role.Name);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}",
                        role.Name,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    /// <summary>
    /// Seeds default users
    /// </summary>
    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        // Admin user
        var adminEmail = "admin@easybuy.com";
        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Manager user
        var managerEmail = "manager@easybuy.com";
        if (await _userManager.FindByEmailAsync(managerEmail) == null)
        {
            var manager = new AppUser
            {
                UserName = "manager",
                Email = managerEmail,
                FirstName = "Store",
                LastName = "Manager",
                EmailConfirmed = true,
                PhoneNumber = "+1234567891",
                PhoneNumberConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(manager, "Manager@123456");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(manager, "Manager");
                _logger.LogInformation("Created manager user: {Email}", managerEmail);
            }
        }

        // Customer user
        var customerEmail = "customer@easybuy.com";
        if (await _userManager.FindByEmailAsync(customerEmail) == null)
        {
            var customer = new AppUser
            {
                UserName = "customer",
                Email = customerEmail,
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                PhoneNumber = "+1234567892",
                PhoneNumberConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(customer, "Customer@123456");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(customer, "Customer");
                _logger.LogInformation("Created customer user: {Email}", customerEmail);
            }
        }
    }

    /// <summary>
    /// Seeds product categories
    /// </summary>
    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist, skipping seed");
            return;
        }

        _logger.LogInformation("Seeding categories...");

        var categories = new[]
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Description = "Apparel and fashion items",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Description = "Physical and digital books",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Home & Garden",
                Description = "Home improvement and garden supplies",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Sports & Outdoors",
                Description = "Sports equipment and outdoor gear",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} categories", categories.Length);
    }

    /// <summary>
    /// Seeds sample products
    /// </summary>
    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist, skipping seed");
            return;
        }

        _logger.LogInformation("Seeding products...");

        var electronicsCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        var clothingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Clothing");
        var booksCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Books");

        var products = new List<Product>();

        if (electronicsCategory != null)
        {
            products.AddRange(new[]
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop Pro 15",
                    Description = "High-performance laptop with 15-inch display, 16GB RAM, 512GB SSD",
                    Price = 1299.99m,
                    Stock = 50,
                    CategoryId = electronicsCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with 6 programmable buttons",
                    Price = 29.99m,
                    Stock = 200,
                    CategoryId = electronicsCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "4K Monitor 27\"",
                    Description = "Ultra HD 4K monitor with HDR support",
                    Price = 399.99m,
                    Stock = 75,
                    CategoryId = electronicsCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            });
        }

        if (clothingCategory != null)
        {
            products.AddRange(new[]
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Cotton T-Shirt",
                    Description = "100% cotton comfortable t-shirt, available in multiple colors",
                    Price = 19.99m,
                    Stock = 500,
                    CategoryId = clothingCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Denim Jeans",
                    Description = "Classic fit denim jeans",
                    Price = 49.99m,
                    Stock = 300,
                    CategoryId = clothingCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            });
        }

        if (booksCategory != null)
        {
            products.AddRange(new[]
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Clean Code",
                    Description = "A Handbook of Agile Software Craftsmanship by Robert C. Martin",
                    Price = 34.99m,
                    Stock = 150,
                    CategoryId = booksCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Design Patterns",
                    Description = "Elements of Reusable Object-Oriented Software",
                    Price = 39.99m,
                    Stock = 100,
                    CategoryId = booksCategory.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            });
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} products", products.Count);
    }

    /// <summary>
    /// Seeds sample coupons
    /// </summary>
    private async Task SeedCouponsAsync()
    {
        if (await _context.Coupons.AnyAsync())
        {
            _logger.LogInformation("Coupons already exist, skipping seed");
            return;
        }

        _logger.LogInformation("Seeding coupons...");

        var coupons = new[]
        {
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "WELCOME10",
                Description = "10% off for new customers",
                DiscountType = Domain.Enums.DiscountType.Percentage,
                DiscountValue = 10,
                MinimumOrderAmount = 50,
                MaximumDiscountAmount = 20,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                UsageLimit = 1000,
                UsageCount = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "SAVE20",
                Description = "$20 off on orders over $100",
                DiscountType = Domain.Enums.DiscountType.FixedAmount,
                DiscountValue = 20,
                MinimumOrderAmount = 100,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 500,
                UsageCount = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "FREESHIP",
                Description = "Free shipping on all orders",
                DiscountType = Domain.Enums.DiscountType.FreeShipping,
                DiscountValue = 0,
                MinimumOrderAmount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                UsageLimit = null, // Unlimited
                UsageCount = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        await _context.Coupons.AddRangeAsync(coupons);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} coupons", coupons.Length);
    }
}

/// <summary>
/// Extension methods for seeding the database
/// </summary>
public static class DatabaseSeederExtensions
{
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<EasyBuyDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

            var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
