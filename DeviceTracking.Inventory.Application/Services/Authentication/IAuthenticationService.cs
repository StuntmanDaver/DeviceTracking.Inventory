using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;

namespace DeviceTracking.Inventory.Application.Services.Authentication;

/// <summary>
/// Authentication service interface for Device Tracking integration
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate user with PBKDF2-based credentials
    /// </summary>
    Task<ServiceResult<AuthenticationResult>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate authentication token
    /// </summary>
    Task<ServiceResult<ClaimsPrincipal>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate authentication token for user
    /// </summary>
    Task<ServiceResult<string>> GenerateTokenAsync(Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user roles and permissions
    /// </summary>
    Task<ServiceResult<UserPermissions>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate user has required permission
    /// </summary>
    Task<ServiceResult> ValidatePermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    Task<ServiceResult<string>> RefreshTokenAsync(string expiredToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout user (invalidate token)
    /// </summary>
    Task<ServiceResult> LogoutAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    Task<ServiceResult<UserInfo>> GetCurrentUserAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Authentication result
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Whether authentication was successful
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// User ID if authenticated
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Authentication token
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// User roles
    /// </summary>
    public IEnumerable<string>? Roles { get; set; }

    /// <summary>
    /// User permissions
    /// </summary>
    public IEnumerable<string>? Permissions { get; set; }

    /// <summary>
    /// Authentication error message
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// User permissions and roles
/// </summary>
public class UserPermissions
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User roles
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// User permissions
    /// </summary>
    public IEnumerable<string> Permissions { get; set; } = new List<string>();

    /// <summary>
    /// Whether user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether user is locked out
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// User information
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User roles
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// User permissions
    /// </summary>
    public IEnumerable<string> Permissions { get; set; } = new List<string>();

    /// <summary>
    /// Whether user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Inventory-specific user roles
/// </summary>
public static class InventoryRoles
{
    /// <summary>
    /// Read-only access to inventory data
    /// </summary>
    public const string Viewer = "InventoryViewer";

    /// <summary>
    /// Basic inventory operations (receipt, issue, transfer)
    /// </summary>
    public const string Clerk = "InventoryClerk";

    /// <summary>
    /// Full inventory management (adjustments, reports, supplier management)
    /// </summary>
    public const string Manager = "InventoryManager";

    /// <summary>
    /// System administration (configuration, user management)
    /// </summary>
    public const string Admin = "InventoryAdmin";
}

/// <summary>
/// Inventory-specific permissions
/// </summary>
public static class InventoryPermissions
{
    // Item management
    public const string CreateItems = "inventory.items.create";
    public const string ReadItems = "inventory.items.read";
    public const string UpdateItems = "inventory.items.update";
    public const string DeleteItems = "inventory.items.delete";

    // Location management
    public const string CreateLocations = "inventory.locations.create";
    public const string ReadLocations = "inventory.locations.read";
    public const string UpdateLocations = "inventory.locations.update";
    public const string DeleteLocations = "inventory.locations.delete";

    // Transaction management
    public const string CreateTransactions = "inventory.transactions.create";
    public const string ReadTransactions = "inventory.transactions.read";
    public const string UpdateTransactions = "inventory.transactions.update";
    public const string ApproveTransactions = "inventory.transactions.approve";

    // Supplier management
    public const string CreateSuppliers = "inventory.suppliers.create";
    public const string ReadSuppliers = "inventory.suppliers.read";
    public const string UpdateSuppliers = "inventory.suppliers.update";
    public const string DeleteSuppliers = "inventory.suppliers.delete";

    // Reporting
    public const string ViewReports = "inventory.reports.view";
    public const string ExportReports = "inventory.reports.export";

    // Administration
    public const string ManageUsers = "inventory.admin.users";
    public const string ManageSettings = "inventory.admin.settings";
    public const string ViewAuditLogs = "inventory.admin.audit";
}
