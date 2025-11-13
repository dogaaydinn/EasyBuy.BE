using Asp.Versioning;
using AspNetCoreRateLimit;
using EasyBuy.Application;
using EasyBuy.Infrastructure;
using EasyBuy.Infrastructure.Services.Storage.Azure;
using EasyBuy.Persistence;
using EasyBuy.WebAPI.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting EasyBuy.API");

    // Add services to the container
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });

    // Add layer services
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices();
    builder.Services.AddPersistenceServices(builder.Configuration);

    // Add storage service (Azure by default)
    builder.Services.AddStorage<AzureStorage>();

    // Configure CORS
    var corsConfig = builder.Configuration.GetSection("Cors");
    var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsConfig["PolicyName"] ?? "DefaultCorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods(corsConfig.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE" })
                .WithHeaders(corsConfig.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" });

            if (corsConfig.GetValue<bool>("AllowCredentials"))
            {
                policy.AllowCredentials();
            }
        });
    });

    // Configure Redis Distributed Cache
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
    if (!string.IsNullOrEmpty(redisConnection) && builder.Configuration.GetValue<bool>("FeatureFlags:EnableDistributedCache"))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "EasyBuy_";
        });
    }
    else
    {
        // Fallback to in-memory cache
        builder.Services.AddDistributedMemoryCache();
    }

    // Configure Rate Limiting
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableRateLimiting"))
    {
        builder.Services.AddMemoryCache();
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
        builder.Services.AddInMemoryRateLimiting();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }

    // Configure Response Compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    // Configure API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = builder.Configuration.GetValue<bool>("ApiVersioning:AssumeDefaultVersionWhenUnspecified");
        options.ReportApiVersions = builder.Configuration.GetValue<bool>("ApiVersioning:ReportApiVersions");
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Configure Health Checks
    var healthChecksBuilder = builder.Services.AddHealthChecks();

    // Add database health check
    var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(dbConnection))
    {
        healthChecksBuilder.AddNpgSql(dbConnection, name: "database", tags: new[] { "db", "postgresql" });
    }

    // Add Redis health check
    if (!string.IsNullOrEmpty(redisConnection))
    {
        healthChecksBuilder.AddRedis(redisConnection, name: "redis", tags: new[] { "cache", "redis" });
    }

    // Configure Hangfire for background jobs
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableHangfire"))
    {
        var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection") ?? dbConnection;
        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(c =>
                    c.UseNpgsqlConnection(hangfireConnection));
        });

        builder.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", 5);
        });
    }

    // Configure Swagger/OpenAPI
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableSwagger"))
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "EasyBuy API",
                Version = "v1",
                Description = "EasyBuy E-Commerce Platform API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "EasyBuy Team",
                    Email = "support@easybuy.com"
                }
            });

            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Enable Swagger in development or if explicitly enabled
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableSwagger"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasyBuy API V1");
            c.RoutePrefix = "swagger";
        });
    }

    // Global Exception Handler (must be early in pipeline)
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Correlation ID Middleware
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Security Headers Middleware
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // Response Compression
    app.UseResponseCompression();

    // Static Files
    app.UseStaticFiles();

    // HTTPS Redirection
    app.UseHttpsRedirection();

    // CORS
    app.UseCors(corsConfig["PolicyName"] ?? "DefaultCorsPolicy");

    // Rate Limiting
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableRateLimiting"))
    {
        app.UseIpRateLimiting();
    }

    // Routing
    app.UseRouting();

    // Authentication & Authorization (TODO: Add when Identity is configured)
    // app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Hangfire Dashboard
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableHangfire"))
    {
        var dashboardPath = builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire";
        app.MapHangfireDashboard(dashboardPath, new DashboardOptions
        {
            AppPath = null,
            DashboardTitle = builder.Configuration["Hangfire:DashboardTitle"] ?? "EasyBuy Background Jobs"
            // TODO: Add authorization when Identity is configured
            // Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }

    // Map Controllers
    app.MapControllers();

    // Welcome endpoint
    app.MapGet("/", () => new
    {
        application = "EasyBuy API",
        version = "1.0.0",
        status = "Running",
        timestamp = DateTime.UtcNow,
        endpoints = new
        {
            health = "/health",
            swagger = "/swagger",
            hangfire = builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire"
        }
    });

    Log.Information("EasyBuy.API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}