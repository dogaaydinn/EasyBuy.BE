# Database Migration Guide

This guide provides instructions for creating and applying Entity Framework Core migrations for the EasyBuy.BE project.

## Prerequisites

- .NET 9.0 SDK installed
- PostgreSQL database server running
- Connection string configured in `appsettings.json`

## Creating Migrations

### Initial Migration

To create the initial migration with all entities (Identity tables, Products, Orders, etc.):

```bash
# From the repository root directory
dotnet ef migrations add InitialCreate \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

### Adding Subsequent Migrations

When you make changes to your entity models, create a new migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

Replace `<MigrationName>` with a descriptive name like `AddProductReviews` or `UpdateOrderStatus`.

## Applying Migrations

### Update Database to Latest Migration

```bash
dotnet ef database update \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

### Update to Specific Migration

```bash
dotnet ef database update <MigrationName> \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

### Rollback to Previous Migration

```bash
dotnet ef database update <PreviousMigrationName> \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

## Seeding Initial Data

The project includes a `DatabaseSeeder` class that automatically seeds initial data. To seed the database:

### Option 1: Automatic Seeding on Startup

Add the following to your `Program.cs` (already included in the project):

```csharp
// After app.Build() and before app.Run()
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.SeedDatabaseAsync();
}
```

### Option 2: Manual Seeding

You can manually trigger seeding by calling the extension method:

```csharp
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SeedDatabaseAsync();
```

## Seeded Data

The `DatabaseSeeder` creates the following initial data:

### Roles
- **Admin**: System administrator with full access
- **Manager**: Store manager with elevated privileges
- **Customer**: Regular customer user
- **Vendor**: Product vendor/seller

### Users
| Email                    | Password         | Role     |
|-------------------------|------------------|----------|
| admin@easybuy.com       | Admin@123456     | Admin    |
| manager@easybuy.com     | Manager@123456   | Manager  |
| customer@easybuy.com    | Customer@123456  | Customer |

### Categories
- Electronics
- Clothing
- Books
- Home & Garden
- Sports & Outdoors

### Sample Products
- Laptop Pro 15 ($1,299.99)
- Wireless Mouse ($29.99)
- 4K Monitor 27" ($399.99)
- Cotton T-Shirt ($19.99)
- Denim Jeans ($49.99)
- Clean Code Book ($34.99)
- Design Patterns Book ($39.99)

### Coupons
- **WELCOME10**: 10% off for new customers
- **SAVE20**: $20 off on orders over $100
- **FREESHIP**: Free shipping on all orders

## Migration Best Practices

1. **Always create migrations before deploying**: Never deploy without migrations
2. **Review migration code**: Check the generated migration file before applying
3. **Backup database**: Always backup production database before applying migrations
4. **Test migrations**: Test on development/staging environment first
5. **Rollback plan**: Have a rollback plan ready for production migrations

## Troubleshooting

### Error: "Build failed"
Ensure your project builds successfully before creating migrations:
```bash
dotnet build
```

### Error: "Unable to create an object of type 'EasyBuyDbContext'"
Ensure your connection string is properly configured in `appsettings.json`.

### Error: "The specified context could not be found"
Verify the project paths and context name are correct.

### Pending Migrations
To check for pending migrations:
```bash
dotnet ef migrations list \
  --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
  --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
  --context EasyBuyDbContext
```

## Database Schema

After applying migrations, your database will include:

### Identity Tables
- Users
- Roles
- UserRoles
- UserClaims
- UserLogins
- UserTokens
- RoleClaims
- RefreshTokens

### Application Tables
- Products
- Categories
- Orders
- OrderItems
- Baskets
- BasketItems
- Payments
- Reviews
- Addresses
- Wishlists
- Coupons

## Connection String Configuration

Update your `appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EasyBuyDB;Username=postgres;Password=your_password"
  }
}
```

For production, use environment variables or Azure Key Vault to store sensitive connection strings.

## Docker Database Setup

If using Docker, start PostgreSQL with:

```bash
docker-compose up -d postgres
```

The default credentials are defined in `docker-compose.yml`.
