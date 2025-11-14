namespace EasyBuy.WebAPI.Authorization.Policies;

/// <summary>
/// Centralized authorization policy names for the application.
/// Use these constants throughout the application for consistent policy enforcement.
/// </summary>
public static class AuthorizationPolicies
{
    // ====================================================================
    // ROLE-BASED POLICIES
    // ====================================================================

    /// <summary>
    /// Requires user to have Admin role.
    /// Use for: System configuration, user management, global settings
    /// </summary>
    public const string RequireAdminRole = "RequireAdminRole";

    /// <summary>
    /// Requires user to have Manager role (or Admin).
    /// Use for: Product management, order oversight, reports
    /// </summary>
    public const string RequireManagerRole = "RequireManagerRole";

    /// <summary>
    /// Requires user to have Customer role.
    /// Use for: Shopping, reviews, orders
    /// </summary>
    public const string RequireCustomerRole = "RequireCustomerRole";

    // ====================================================================
    // RESOURCE-BASED POLICIES
    // ====================================================================

    /// <summary>
    /// Allows users to manage their own orders.
    /// Admins and Managers can manage all orders.
    /// </summary>
    public const string ManageOrders = "ManageOrders";

    /// <summary>
    /// Allows users to manage products.
    /// Requires Manager or Admin role.
    /// </summary>
    public const string ManageProducts = "ManageProducts";

    /// <summary>
    /// Allows users to manage categories.
    /// Requires Manager or Admin role.
    /// </summary>
    public const string ManageCategories = "ManageCategories";

    /// <summary>
    /// Allows users to manage their own reviews.
    /// Admins and Managers can moderate all reviews.
    /// </summary>
    public const string ManageReviews = "ManageReviews";

    /// <summary>
    /// Allows users to access admin dashboard and features.
    /// Requires Admin role.
    /// </summary>
    public const string AccessAdminDashboard = "AccessAdminDashboard";

    /// <summary>
    /// Allows users to view sales reports.
    /// Requires Manager or Admin role.
    /// </summary>
    public const string ViewReports = "ViewReports";

    // ====================================================================
    // CLAIM-BASED POLICIES
    // ====================================================================

    /// <summary>
    /// Requires email to be verified.
    /// Use for: Critical operations that require verified identity
    /// </summary>
    public const string RequireEmailVerified = "RequireEmailVerified";

    /// <summary>
    /// Requires phone number to be verified.
    /// Use for: High-value transactions, account changes
    /// </summary>
    public const string RequirePhoneVerified = "RequirePhoneVerified";

    /// <summary>
    /// Requires two-factor authentication to be enabled.
    /// Use for: Admin operations, sensitive data access
    /// </summary>
    public const string RequireTwoFactorEnabled = "RequireTwoFactorEnabled";

    // ====================================================================
    // PERMISSION-BASED POLICIES
    // ====================================================================

    /// <summary>
    /// Allows access to Hangfire dashboard.
    /// Requires Admin role.
    /// </summary>
    public const string AccessHangfireDashboard = "AccessHangfireDashboard";

    /// <summary>
    /// Allows access to health check endpoints.
    /// Public for basic /health, restricted for detailed checks.
    /// </summary>
    public const string AccessHealthChecks = "AccessHealthChecks";

    /// <summary>
    /// Allows access to Seq logs.
    /// Requires Admin role.
    /// </summary>
    public const string AccessLogs = "AccessLogs";
}
