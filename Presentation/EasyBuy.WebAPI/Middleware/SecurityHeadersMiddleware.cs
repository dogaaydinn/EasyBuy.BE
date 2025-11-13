namespace EasyBuy.WebAPI.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var securityConfig = _configuration.GetSection("SecurityHeaders");

        // X-Content-Type-Options
        if (securityConfig.GetValue<bool>("EnableXContentTypeOptions"))
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        }

        // X-Frame-Options
        if (securityConfig.GetValue<bool>("EnableXFrameOptions"))
        {
            var xFrameValue = securityConfig.GetValue<string>("XFrameOptionsValue") ?? "DENY";
            context.Response.Headers.Append("X-Frame-Options", xFrameValue);
        }

        // X-XSS-Protection
        if (securityConfig.GetValue<bool>("EnableXXssProtection"))
        {
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        }

        // Referrer-Policy
        if (securityConfig.GetValue<bool>("EnableReferrerPolicy"))
        {
            var referrerValue = securityConfig.GetValue<string>("ReferrerPolicyValue") ?? "no-referrer";
            context.Response.Headers.Append("Referrer-Policy", referrerValue);
        }

        // Content-Security-Policy
        var csp = securityConfig.GetValue<string>("ContentSecurityPolicy");
        if (!string.IsNullOrEmpty(csp))
        {
            context.Response.Headers.Append("Content-Security-Policy", csp);
        }

        // Remove Server header
        context.Response.Headers.Remove("Server");

        await _next(context);
    }
}
