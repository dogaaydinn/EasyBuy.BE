using Asp.Versioning;
using AspNetCoreRateLimit;
using Azure.Identity;
using EasyBuy.Application;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Infrastructure;
using EasyBuy.Infrastructure.Services.Storage.Azure;
using EasyBuy.Persistence;
using EasyBuy.Persistence.Contexts;
using EasyBuy.Persistence.Data;
using EasyBuy.WebAPI.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ENTERPRISE SECRETS MANAGEMENT
// ============================================================================
// Production: Use Azure Key Vault for secure secret storage
// Development: Use User Secrets (right-click project > Manage User Secrets)
// ============================================================================
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        var keyVaultUri = new Uri(keyVaultUrl);

        // Use DefaultAzureCredential which supports:
        // - Managed Identity (production in Azure)
        // - Azure CLI (local development)
        // - Environment variables
        // - Visual Studio authentication
        builder.Configuration.AddAzureKeyVault(
            keyVaultUri,
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                // Exclude interactive authentication for production
                ExcludeInteractiveBrowserCredential = true,
                // Prioritize Managed Identity in Azure
                ManagedIdentityClientId = builder.Configuration["KeyVault:ManagedIdentityClientId"]
            }));

        Log.Information("Azure Key Vault configured: {KeyVaultUrl}", keyVaultUrl);
    }
    else
    {
        Log.Warning("Production environment detected but KeyVault:Url not configured. Using appsettings.");
    }
}

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

    // ============================================================================
    // FEATURE MANAGEMENT
    // ============================================================================
    // Microsoft.FeatureManagement provides advanced feature flags with:
    // - Percentage rollouts
    // - Time windows
    // - Custom filters
    // - A/B testing
    builder.Services.AddFeatureManagement();
    Log.Information("Feature Management configured");

    // ============================================================================
    // GRAPHQL API
    // ============================================================================
    // HotChocolate GraphQL server provides:
    // - Type-safe queries and mutations
    // - Automatic schema generation
    // - Built-in filtering, sorting, and pagination
    // - Integration with EF Core
    // - GraphQL IDE (Banana Cake Pop) at /graphql
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableGraphQL"))
    {
        builder.Services
            .AddGraphQLServer()
            .AddQueryType<EasyBuy.WebAPI.GraphQL.Queries.Query>()
            .AddMutationType<EasyBuy.WebAPI.GraphQL.Mutations.Mutation>()
            .AddAuthorization()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .RegisterDbContext<EasyBuyDbContext>(DbContextKind.Pooled);

        Log.Information("GraphQL API configured with HotChocolate and authorization");
    }

    // ============================================================================
    // ELASTICSEARCH FULL-TEXT SEARCH
    // ============================================================================
    // Elasticsearch provides powerful full-text search capabilities:
    // - Product search with fuzzy matching
    // - Autocomplete suggestions
    // - Multi-field search (name, description, category)
    // - Relevance scoring and boosting
    if (builder.Configuration.GetValue<bool>("Elasticsearch:EnableElasticsearch"))
    {
        builder.Services.AddElasticsearch(builder.Configuration);
        Log.Information("Elasticsearch configured: {Url}",
            builder.Configuration["Elasticsearch:Url"]);
    }

    // ============================================================================
    // SIGNALR REAL-TIME NOTIFICATIONS
    // ============================================================================
    // SignalR provides WebSocket-based real-time communication:
    // - Order status updates
    // - Product stock alerts
    // - Payment confirmations
    // - Shipment tracking
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableSignalR", false))
    {
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.MaximumReceiveMessageSize = 32 * 1024; // 32 KB
        });

        Log.Information("SignalR real-time notifications configured");
    }

    // Configure ASP.NET Core Identity
    builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = builder.Configuration.GetValue<bool>("Identity:Password:RequireDigit", true);
        options.Password.RequireLowercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireLowercase", true);
        options.Password.RequireUppercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireUppercase", true);
        options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric", true);
        options.Password.RequiredLength = builder.Configuration.GetValue<int>("Identity:Password:RequiredLength", 8);
        options.Password.RequiredUniqueChars = builder.Configuration.GetValue<int>("Identity:Password:RequiredUniqueChars", 1);

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Identity:Lockout:DefaultLockoutTimeSpanMinutes", 5));
        options.Lockout.MaxFailedAccessAttempts = builder.Configuration.GetValue<int>("Identity:Lockout:MaxFailedAccessAttempts", 5);
        options.Lockout.AllowedForNewUsers = builder.Configuration.GetValue<bool>("Identity:Lockout:AllowedForNewUsers", true);

        // User settings
        options.User.RequireUniqueEmail = builder.Configuration.GetValue<bool>("Identity:User:RequireUniqueEmail", true);
        options.SignIn.RequireConfirmedEmail = builder.Configuration.GetValue<bool>("Identity:SignIn:RequireConfirmedEmail", false);
        options.SignIn.RequireConfirmedPhoneNumber = builder.Configuration.GetValue<bool>("Identity:SignIn:RequireConfirmedPhoneNumber", false);
    })
    .AddEntityFrameworkStores<EasyBuyDbContext>()
    .AddDefaultTokenProviders();

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
    }

    var key = Encoding.UTF8.GetBytes(secretKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
        };

        // Add logging for authentication failures
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT Authentication challenge: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

    // ============================================================================
    // COMPREHENSIVE AUTHORIZATION POLICIES
    // ============================================================================
    // Register authorization handlers
    builder.Services.AddScoped<IAuthorizationHandler, EasyBuy.WebAPI.Authorization.Requirements.EmailVerifiedHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, EasyBuy.WebAPI.Authorization.Requirements.ManageOrderHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, EasyBuy.WebAPI.Authorization.Requirements.ManageReviewHandler>();

    builder.Services.AddAuthorization(options =>
    {
        // ====================================================================
        // ROLE-BASED POLICIES
        // ====================================================================
        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequireAdminRole,
            policy => policy.RequireRole("Admin"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequireManagerRole,
            policy => policy.RequireRole("Admin", "Manager"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequireCustomerRole,
            policy => policy.RequireRole("Customer"));

        // ====================================================================
        // RESOURCE-BASED POLICIES
        // ====================================================================
        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.ManageProducts,
            policy => policy.RequireRole("Admin", "Manager"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.ManageCategories,
            policy => policy.RequireRole("Admin", "Manager"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.ManageOrders,
            policy => policy.AddRequirements(new EasyBuy.WebAPI.Authorization.Requirements.ManageOrderRequirement()));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.ManageReviews,
            policy => policy.AddRequirements(new EasyBuy.WebAPI.Authorization.Requirements.ManageReviewRequirement()));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.AccessAdminDashboard,
            policy => policy.RequireRole("Admin"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.ViewReports,
            policy => policy.RequireRole("Admin", "Manager"));

        // ====================================================================
        // CLAIM-BASED POLICIES
        // ====================================================================
        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequireEmailVerified,
            policy => policy.AddRequirements(new EasyBuy.WebAPI.Authorization.Requirements.EmailVerifiedRequirement()));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequirePhoneVerified,
            policy => policy.RequireClaim("PhoneNumberConfirmed", "true"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.RequireTwoFactorEnabled,
            policy => policy.RequireClaim("TwoFactorEnabled", "true"));

        // ====================================================================
        // SERVICE ACCESS POLICIES
        // ====================================================================
        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.AccessHangfireDashboard,
            policy => policy.RequireRole("Admin"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.AccessHealthChecks,
            policy => policy.RequireRole("Admin", "Manager"));

        options.AddPolicy(EasyBuy.WebAPI.Authorization.Policies.AuthorizationPolicies.AccessLogs,
            policy => policy.RequireRole("Admin"));
    });

    Log.Information("Comprehensive authorization policies configured");

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

    // ====================================================================
    // MULTI-LEVEL CACHING CONFIGURATION
    // ====================================================================
    // L1: In-memory cache with size limit (10K entries max)
    builder.Services.AddMemoryCache(options =>
    {
        options.SizeLimit = 10_000; // Maximum 10K cache entries
        options.CompactionPercentage = 0.25; // Remove 25% when limit reached
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(1); // Scan for expired entries every minute
    });

    // L2: Redis Distributed Cache
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
    if (!string.IsNullOrEmpty(redisConnection) && builder.Configuration.GetValue<bool>("FeatureFlags:EnableDistributedCache"))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "EasyBuy_";
        });
        Log.Information("Redis L2 cache configured: {RedisConnection}", redisConnection.Split(',')[0]);
    }
    else
    {
        // Fallback to in-memory cache
        builder.Services.AddDistributedMemoryCache();
        Log.Warning("Redis not configured - using in-memory distributed cache fallback");
    }

    // ====================================================================
    // ENHANCED RATE LIMITING (IP + Client-Based)
    // ====================================================================
    // Multi-tier rate limiting based on user roles:
    // - Admin: 50 req/s, 500 req/min
    // - Premium/Manager: 20 req/s, 200 req/min
    // - Regular: 10 req/s, 100 req/min
    // - Anonymous: IP-based limits (strictest)
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableRateLimiting"))
    {
        // Configure IP-based rate limiting
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
        builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));

        // Register in-memory stores (for single instance) or distributed stores (for multi-instance)
        builder.Services.AddInMemoryRateLimiting();

        // Register custom client ID resolver (maps users to rate limit tiers by role)
        builder.Services.AddSingleton<IClientResolveContributor, EasyBuy.WebAPI.RateLimiting.UserRoleClientIdResolveContributor>();

        // Register rate limit configuration
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        Log.Information("Enhanced rate limiting configured: IP-based + Client-based with role-based tiers");
    }

    // ====================================================================
    // DISTRIBUTED MESSAGING (RabbitMQ + MassTransit)
    // ====================================================================
    if (builder.Configuration.GetValue<bool>("RabbitMQ:EnableRabbitMQ"))
    {
        Log.Information("Configuring MassTransit with RabbitMQ");
        builder.Services.AddMassTransitMessaging(builder.Configuration);
    }
    else
    {
        Log.Information("RabbitMQ messaging disabled");
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

    // Seed database in development environment
    if (app.Environment.IsDevelopment())
    {
        try
        {
            Log.Information("Seeding database...");
            await app.Services.SeedDatabaseAsync();
            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database. Continuing startup...");
        }
    }

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

    // Authentication & Authorization
    app.UseAuthentication();
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

        // Schedule recurring background jobs
        using var scope = app.Services.CreateScope();
        var jobScheduler = scope.ServiceProvider.GetRequiredService<EasyBuy.Infrastructure.BackgroundJobs.JobScheduler>();
        jobScheduler.ScheduleRecurringJobs();
        Log.Information("Hangfire recurring jobs scheduled successfully");
    }

    // GraphQL Endpoint
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableGraphQL"))
    {
        app.MapGraphQL("/graphql");
        Log.Information("GraphQL endpoint mapped to /graphql");
    }

    // SignalR Hubs
    if (builder.Configuration.GetValue<bool>("FeatureFlags:EnableSignalR", false))
    {
        app.MapHub<EasyBuy.WebAPI.Hubs.NotificationHub>("/hubs/notifications");
        app.MapHub<EasyBuy.WebAPI.Hubs.OrderHub>("/hubs/orders");
        Log.Information("SignalR hubs mapped: /hubs/notifications, /hubs/orders");
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
            hangfire = builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire",
            graphql = builder.Configuration.GetValue<bool>("FeatureFlags:EnableGraphQL") ? "/graphql" : null,
            signalr = builder.Configuration.GetValue<bool>("FeatureFlags:EnableSignalR", false) ? new
            {
                notifications = "/hubs/notifications",
                orders = "/hubs/orders"
            } : null
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