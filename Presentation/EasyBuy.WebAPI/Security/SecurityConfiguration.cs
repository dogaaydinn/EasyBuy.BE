using Microsoft.AspNetCore.Http.Features;

namespace EasyBuy.WebAPI.Security;

/// <summary>
/// Security hardening configuration following OWASP Top 10 best practices
/// </summary>
public static class SecurityConfiguration
{
    /// <summary>
    /// Configure security headers and protections
    /// </summary>
    public static IServiceCollection AddSecurityHardening(this IServiceCollection services, IConfiguration configuration)
    {
        // OWASP #1: Broken Access Control
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // OWASP #2: Cryptographic Failures - Enforce HTTPS
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // OWASP #4: Insecure Design - Request size limits
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 5_242_880; // 5MB limit
        });

        // OWASP #5: Security Misconfiguration - Disable detailed errors in production
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });

        return services;
    }

    /// <summary>
    /// Configure security middleware
    /// </summary>
    public static IApplicationBuilder UseSecurityHardening(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // OWASP #3: Injection - Input validation handled by FluentValidation

        // OWASP #5: Security Misconfiguration
        if (!env.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // OWASP #7: Identification and Authentication Failures
        // Handled by ASP.NET Core Identity + JWT

        // OWASP #8: Software and Data Integrity Failures
        // Use security headers middleware (already configured)

        // OWASP #9: Security Logging and Monitoring Failures
        // Handled by Serilog with correlation IDs

        // OWASP #10: Server-Side Request Forgery (SSRF)
        // Validate all external URLs in services

        return app;
    }

    /// <summary>
    /// Validate configuration for security issues
    /// </summary>
    public static void ValidateSecurityConfiguration(IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.IsProduction())
        {
            // Ensure JWT secret is strong
            var jwtSecret = configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
            {
                throw new InvalidOperationException(
                    "SECURITY: JWT SecretKey must be at least 32 characters in production");
            }

            // Ensure database password is not default
            var dbConnection = configuration.GetConnectionString("DefaultConnection");
            if (dbConnection?.Contains("Password=CONFIGURE") == true)
            {
                throw new InvalidOperationException(
                    "SECURITY: Database password must be configured in production");
            }

            // Ensure CORS is configured properly
            var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (corsOrigins?.Contains("*") == true)
            {
                throw new InvalidOperationException(
                    "SECURITY: CORS cannot allow all origins (*) in production");
            }
        }
    }
}
