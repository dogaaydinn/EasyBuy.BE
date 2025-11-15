using System.Text.RegularExpressions;

namespace EasyBuy.WebAPI.Security;

/// <summary>
/// Input sanitization to prevent XSS and injection attacks
/// </summary>
public static class InputSanitization
{
    private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|SCRIPT|UNION)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Sanitize string input to prevent XSS attacks
    /// </summary>
    public static string? SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove HTML tags
        var sanitized = HtmlTagPattern.Replace(input, string.Empty);

        // Decode HTML entities
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

        return sanitized.Trim();
    }

    /// <summary>
    /// Check for potential SQL injection patterns (defense in depth)
    /// Note: Parameterized queries (EF Core) are primary defense
    /// </summary>
    public static bool ContainsSqlInjectionPattern(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return SqlInjectionPattern.IsMatch(input);
    }

    /// <summary>
    /// Sanitize user-provided URLs
    /// </summary>
    public static bool IsValidUrl(string? url, bool httpsOnly = true)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (httpsOnly && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // Prevent SSRF to internal networks
        if (uri.IsLoopback || uri.Host == "localhost" || uri.Host.StartsWith("192.168.") ||
            uri.Host.StartsWith("10.") || uri.Host.StartsWith("172."))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validate file upload
    /// </summary>
    public static bool IsValidFileUpload(IFormFile file, string[] allowedExtensions, long maxSizeBytes)
    {
        if (file == null || file.Length == 0)
            return false;

        // Check file size
        if (file.Length > maxSizeBytes)
            return false;

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return false;

        // Check MIME type matches extension
        var expectedMimeTypes = GetMimeTypesForExtension(extension);
        if (!expectedMimeTypes.Contains(file.ContentType))
            return false;

        return true;
    }

    private static string[] GetMimeTypesForExtension(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => new[] { "image/jpeg" },
            ".png" => new[] { "image/png" },
            ".gif" => new[] { "image/gif" },
            ".pdf" => new[] { "application/pdf" },
            ".doc" => new[] { "application/msword" },
            ".docx" => new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            _ => Array.Empty<string>()
        };
    }
}
