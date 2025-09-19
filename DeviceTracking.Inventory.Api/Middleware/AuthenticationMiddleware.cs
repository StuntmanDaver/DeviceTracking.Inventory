using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DeviceTracking.Inventory.Api.Middleware;

/// <summary>
/// Authentication middleware for JWT token validation
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<AuthenticationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromRequest(context);

        if (!string.IsNullOrEmpty(token))
        {
            var claimsPrincipal = await ValidateTokenAsync(token, context);
            if (claimsPrincipal != null)
            {
                context.User = claimsPrincipal;
                _logger.LogDebug("Authenticated user: {UserId}", claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromRequest(HttpContext context)
    {
        // Try Authorization header first
        var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization.Substring("Bearer ".Length).Trim();
        }

        // Try query parameter
        var tokenParam = context.Request.Query["token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenParam))
        {
            return tokenParam;
        }

        // Try cookie
        var tokenCookie = context.Request.Cookies["auth_token"];
        if (!string.IsNullOrEmpty(tokenCookie))
        {
            return tokenCookie;
        }

        return null;
    }

    private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, HttpContext context)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnly");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "DeviceTracking.Inventory",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "DeviceTracking.Inventory.Api",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Additional validation - check user status
            var authService = context.RequestServices.GetRequiredService<IAuthenticationService>();
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var permissionsResult = await authService.GetUserPermissionsAsync(userId);
                if (permissionsResult.IsSuccess && permissionsResult.Data != null)
                {
                    var userPermissions = permissionsResult.Data;
                    if (!userPermissions.IsActive || userPermissions.IsLockedOut)
                    {
                        _logger.LogWarning("Token validation failed - user {UserId} is disabled or locked", userId);
                        return null;
                    }

                    // Add permissions as claims for authorization
                    var identity = principal.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        foreach (var permission in userPermissions.Permissions)
                        {
                            identity.AddClaim(new Claim("permission", permission));
                        }
                    }
                }
            }

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("Invalid token signature");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return null;
        }
    }
}

/// <summary>
/// Extension methods for authentication middleware
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Add authentication middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}

/// <summary>
/// Authorization requirement for inventory permissions
/// </summary>
public class InventoryPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public InventoryPermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Authorization handler for inventory permissions
/// </summary>
public class InventoryPermissionHandler : AuthorizationHandler<InventoryPermissionRequirement>
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<InventoryPermissionHandler> _logger;

    public InventoryPermissionHandler(
        IAuthenticationService authService,
        ILogger<InventoryPermissionHandler> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InventoryPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated");
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid user ID in token");
            return;
        }

        // Check if user has the required permission
        var hasPermission = context.User.HasClaim("permission", requirement.Permission);

        if (hasPermission)
        {
            _logger.LogDebug("User {UserId} has permission {Permission}", userId, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} does not have permission {Permission}", userId, requirement.Permission);
        }
    }
}

/// <summary>
/// Extension methods for inventory authorization
/// </summary>
public static class InventoryAuthorizationExtensions
{
    /// <summary>
    /// Add inventory authorization policies
    /// </summary>
    public static IServiceCollection AddInventoryAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Item management policies
            options.AddPolicy("CanCreateItems", policy =>
                policy.RequireClaim("permission", InventoryPermissions.CreateItems));
            options.AddPolicy("CanReadItems", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ReadItems));
            options.AddPolicy("CanUpdateItems", policy =>
                policy.RequireClaim("permission", InventoryPermissions.UpdateItems));
            options.AddPolicy("CanDeleteItems", policy =>
                policy.RequireClaim("permission", InventoryPermissions.DeleteItems));

            // Location management policies
            options.AddPolicy("CanCreateLocations", policy =>
                policy.RequireClaim("permission", InventoryPermissions.CreateLocations));
            options.AddPolicy("CanReadLocations", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ReadLocations));
            options.AddPolicy("CanUpdateLocations", policy =>
                policy.RequireClaim("permission", InventoryPermissions.UpdateLocations));
            options.AddPolicy("CanDeleteLocations", policy =>
                policy.RequireClaim("permission", InventoryPermissions.DeleteLocations));

            // Transaction management policies
            options.AddPolicy("CanCreateTransactions", policy =>
                policy.RequireClaim("permission", InventoryPermissions.CreateTransactions));
            options.AddPolicy("CanReadTransactions", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ReadTransactions));
            options.AddPolicy("CanUpdateTransactions", policy =>
                policy.RequireClaim("permission", InventoryPermissions.UpdateTransactions));
            options.AddPolicy("CanApproveTransactions", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ApproveTransactions));

            // Supplier management policies
            options.AddPolicy("CanCreateSuppliers", policy =>
                policy.RequireClaim("permission", InventoryPermissions.CreateSuppliers));
            options.AddPolicy("CanReadSuppliers", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ReadSuppliers));
            options.AddPolicy("CanUpdateSuppliers", policy =>
                policy.RequireClaim("permission", InventoryPermissions.UpdateSuppliers));
            options.AddPolicy("CanDeleteSuppliers", policy =>
                policy.RequireClaim("permission", InventoryPermissions.DeleteSuppliers));

            // Reporting policies
            options.AddPolicy("CanViewReports", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ViewReports));
            options.AddPolicy("CanExportReports", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ExportReports));

            // Administration policies
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ManageUsers));
            options.AddPolicy("CanManageSettings", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ManageSettings));
            options.AddPolicy("CanViewAuditLogs", policy =>
                policy.RequireClaim("permission", InventoryPermissions.ViewAuditLogs));
        });

        services.AddScoped<IAuthorizationHandler, InventoryPermissionHandler>();

        return services;
    }
}

/// <summary>
/// Session management service for shared authentication with Device Tracking
/// </summary>
public class SessionManagementService
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<SessionManagementService> _logger;

    public SessionManagementService(
        IAuthenticationService authService,
        ILogger<SessionManagementService> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validate and refresh session if needed
    /// </summary>
    public async Task<bool> ValidateSessionAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // Validate token
        var validationResult = await _authService.ValidateTokenAsync(token);
        if (!validationResult.IsSuccess)
        {
            return false;
        }

        // Check if token is close to expiry and refresh if needed
        var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
        if (jwtToken != null)
        {
            var expiryTime = jwtToken.ValidTo;
            var timeToExpiry = expiryTime - DateTime.UtcNow;

            // If token expires in less than 30 minutes, refresh it
            if (timeToExpiry < TimeSpan.FromMinutes(30))
            {
                var refreshResult = await _authService.RefreshTokenAsync(token);
                if (refreshResult.IsSuccess)
                {
                    // Set new token in response header
                    context.Response.Headers["X-New-Token"] = refreshResult.Data;
                    _logger.LogDebug("Token refreshed for user");
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Create session cookie for web clients
    /// </summary>
    public void CreateSessionCookie(HttpContext context, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(8)
        };

        context.Response.Cookies.Append("auth_token", token, cookieOptions);
    }

    /// <summary>
    /// Clear session cookie
    /// </summary>
    public void ClearSessionCookie(HttpContext context)
    {
        context.Response.Cookies.Delete("auth_token");
    }
}
