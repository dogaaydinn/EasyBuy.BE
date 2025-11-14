using EasyBuy.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace EasyBuy.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing with Testcontainers.
/// Provides isolated test environment with real PostgreSQL and Redis instances.
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private Respawner _respawner = null!;
    private string _connectionString = null!;

    public WebApplicationFactoryFixture()
    {
        // Initialize Testcontainers
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("easybuy_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public HttpClient HttpClient { get; private set; } = null!;

    /// <summary>
    /// Initialize containers and database before tests.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start containers in parallel
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync());

        _connectionString = _postgresContainer.GetConnectionString();

        // Initialize database schema
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EasyBuyDbContext>();
        await dbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup between tests
        await using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });

        // Create HttpClient
        HttpClient = CreateClient();
    }

    /// <summary>
    /// Reset database state between tests for isolation.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EasyBuyDbContext>();
        await using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Configure test services, replacing real services with test doubles.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove real DbContext
            services.RemoveAll<DbContextOptions<EasyBuyDbContext>>();
            services.RemoveAll<EasyBuyDbContext>();

            // Add test DbContext with Testcontainers connection string
            services.AddDbContext<EasyBuyDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            // Replace Redis with test container
            services.RemoveAll<IDistributedCache>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
            });

            // Disable authentication for integration tests (optional)
            // services.AddAuthentication("Test")
            //     .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Disable background services that might interfere with tests
            // services.RemoveAll(typeof(IHostedService));

            // Override configuration
            services.PostConfigure<Microsoft.AspNetCore.Hosting.HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(1);
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Cleanup containers after all tests complete.
    /// </summary>
    public new async Task DisposeAsync()
    {
        HttpClient?.Dispose();

        await Task.WhenAll(
            _postgresContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask());

        await base.DisposeAsync();
    }
}
