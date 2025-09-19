using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DeviceTracking.Inventory.Application.Services.Authentication;

/// <summary>
/// PBKDF2-based authentication service integrated with Device Tracking platform
/// </summary>
public class DeviceTrackingAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeviceTrackingAuthenticationService> _logger;

    // PBKDF2 parameters (matching Device Tracking)
    private const int SaltSize = 32; // 256 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 10000;

    public DeviceTrackingAuthenticationService(
        IConfiguration configuration,
        ILogger<DeviceTrackingAuthenticationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AuthenticationResult>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult<AuthenticationResult>.Failure("Username and password are required");
            }

            // Get user from Device Tracking shared authentication
            var userResult = await GetUserFromDeviceTrackingAsync(username, cancellationToken);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResult<AuthenticationResult>.Failure("Invalid username or password");
            }

            var user = userResult.Data;

            // Verify password using PBKDF2
            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Failed login attempt for user {Username}", username);
                return ServiceResult<AuthenticationResult>.Failure("Invalid username or password");
            }

            // Check if user is active and not locked out
            if (!user.IsActive)
            {
                return ServiceResult<AuthenticationResult>.Failure("Account is disabled");
            }

            if (user.IsLockedOut)
            {
                return ServiceResult<AuthenticationResult>.Failure("Account is locked out");
            }

            // Get user roles and permissions
            var permissionsResult = await GetUserPermissionsAsync(user.UserId, cancellationToken);
            if (!permissionsResult.IsSuccess)
            {
                return ServiceResult<AuthenticationResult>.Failure("Failed to load user permissions");
            }

            var permissions = permissionsResult.Data!;

            // Generate JWT token
            var tokenResult = await GenerateTokenAsync(user.UserId, permissions.Roles, cancellationToken);
            if (!tokenResult.IsSuccess)
            {
                return ServiceResult<AuthenticationResult>.Failure("Failed to generate authentication token");
            }

            // Update last login
            await UpdateLastLoginAsync(user.UserId, cancellationToken);

            _logger.LogInformation("Successful login for user {Username}", username);

            return ServiceResult<AuthenticationResult>.Success(new AuthenticationResult
            {
                IsAuthenticated = true,
                UserId = user.UserId,
                Username = user.Username,
                Token = tokenResult.Data,
                ExpiresAt = DateTime.UtcNow.AddHours(8), // 8 hour token expiry
                Roles = permissions.Roles,
                Permissions = permissions.Permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            return ServiceResult<AuthenticationResult>.Failure("Authentication service error");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ClaimsPrincipal>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
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

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            // Additional validation - check if user is still active
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var userResult = await GetUserFromDeviceTrackingAsync(userId, cancellationToken);
                if (!userResult.IsSuccess || !userResult.Data!.IsActive || userResult.Data.IsLockedOut)
                {
                    return ServiceResult<ClaimsPrincipal>.Failure("User account is disabled or locked");
                }
            }

            return ServiceResult<ClaimsPrincipal>.Success(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            return ServiceResult<ClaimsPrincipal>.Failure("Token has expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return ServiceResult<ClaimsPrincipal>.Failure("Invalid token");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> GenerateTokenAsync(Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnly");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"] ?? "DeviceTracking.Inventory",
                Audience = _configuration["Jwt:Audience"] ?? "DeviceTracking.Inventory.Api",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return ServiceResult<string>.Success(tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user {UserId}", userId);
            return ServiceResult<string>.Failure("Failed to generate authentication token");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserPermissions>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user from Device Tracking
            var userResult = await GetUserFromDeviceTrackingAsync(userId, cancellationToken);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResult<UserPermissions>.Failure("User not found");
            }

            var user = userResult.Data;

            // Map Device Tracking roles to Inventory roles
            var inventoryRoles = MapDeviceTrackingRolesToInventory(user.Roles);
            var permissions = GetPermissionsForRoles(inventoryRoles);

            return ServiceResult<UserPermissions>.Success(new UserPermissions
            {
                UserId = user.UserId,
                Username = user.Username,
                Roles = inventoryRoles,
                Permissions = permissions,
                IsActive = user.IsActive,
                IsLockedOut = user.IsLockedOut,
                LastLoginAt = user.LastLoginAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return ServiceResult<UserPermissions>.Failure("Failed to load user permissions");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ValidatePermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var permissionsResult = await GetUserPermissionsAsync(userId, cancellationToken);
        if (!permissionsResult.IsSuccess)
        {
            return ServiceResult.Failure("Failed to validate permissions");
        }

        var permissions = permissionsResult.Data!;
        if (!permissions.Permissions.Contains(permission))
        {
            return ServiceResult.Failure($"User does not have permission: {permission}");
        }

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> RefreshTokenAsync(string expiredToken, CancellationToken cancellationToken = default)
    {
        // Validate the expired token
        var validationResult = await ValidateTokenAsync(expiredToken, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return ServiceResult<string>.Failure("Invalid or expired token");
        }

        // Extract user information from token
        var principal = validationResult.Data!;
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

        if (!userIdClaim?.Value is string userIdString || !Guid.TryParse(userIdString, out var userId))
        {
            return ServiceResult<string>.Failure("Invalid token format");
        }

        // Generate new token
        return await GenerateTokenAsync(userId, roles, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        // In a more sophisticated implementation, you might add the token to a blacklist
        // For now, we just log the logout
        _logger.LogInformation("User logged out");
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserInfo>> GetCurrentUserAsync(string token, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateTokenAsync(token, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return ServiceResult<UserInfo>.Failure("Invalid token");
        }

        var principal = validationResult.Data!;
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (!userIdClaim?.Value is string userIdString || !Guid.TryParse(userIdString, out var userId))
        {
            return ServiceResult<UserInfo>.Failure("Invalid token format");
        }

        var userResult = await GetUserFromDeviceTrackingAsync(userId, cancellationToken);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return ServiceResult<UserInfo>.Failure("User not found");
        }

        var user = userResult.Data;
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);

        return ServiceResult<UserInfo>.Success(new UserInfo
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Roles = permissions.IsSuccess ? permissions.Data!.Roles : new List<string>(),
            Permissions = permissions.IsSuccess ? permissions.Data!.Permissions : new List<string>(),
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        });
    }

    // Private helper methods

    private async Task<ServiceResult<DeviceTrackingUser>> GetUserFromDeviceTrackingAsync(string username, CancellationToken cancellationToken)
    {
        // This would integrate with the Device Tracking platform's user service
        // For now, return a mock implementation
        return ServiceResult<DeviceTrackingUser>.Success(new DeviceTrackingUser
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = HashPassword("password"), // Mock password hash
            PasswordSalt = new byte[SaltSize], // Mock salt
            IsActive = true,
            IsLockedOut = false,
            LastLoginAt = DateTime.UtcNow,
            Roles = new[] { "Admin" } // Mock roles
        });
    }

    private async Task<ServiceResult<DeviceTrackingUser>> GetUserFromDeviceTrackingAsync(Guid userId, CancellationToken cancellationToken)
    {
        // This would integrate with the Device Tracking platform's user service
        // For now, return a mock implementation
        return ServiceResult<DeviceTrackingUser>.Success(new DeviceTrackingUser
        {
            UserId = userId,
            Username = "mockuser",
            PasswordHash = new byte[KeySize],
            PasswordSalt = new byte[SaltSize],
            IsActive = true,
            IsLockedOut = false,
            LastLoginAt = DateTime.UtcNow,
            Roles = new[] { "Admin" }
        });
    }

    private async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken)
    {
        // This would update the last login timestamp in Device Tracking
        _logger.LogInformation("Updated last login for user {UserId}", userId);
    }

    private static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(KeySize);
        return computedHash.SequenceEqual(storedHash);
    }

    private static byte[] HashPassword(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var salt = pbkdf2.Salt;
        var hash = pbkdf2.GetBytes(KeySize);

        // Combine salt and hash for storage
        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        return hashBytes;
    }

    private static IEnumerable<string> MapDeviceTrackingRolesToInventory(IEnumerable<string> deviceTrackingRoles)
    {
        var roleMapping = new Dictionary<string, string>
        {
            ["DeviceTracking.Viewer"] = InventoryRoles.Viewer,
            ["DeviceTracking.Clerk"] = InventoryRoles.Clerk,
            ["DeviceTracking.Manager"] = InventoryRoles.Manager,
            ["DeviceTracking.Admin"] = InventoryRoles.Admin
        };

        return deviceTrackingRoles
            .Select(role => roleMapping.TryGetValue(role, out var inventoryRole) ? inventoryRole : role)
            .Distinct();
    }

    private static IEnumerable<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var permissions = new List<string>();

        foreach (var role in roles)
        {
            switch (role)
            {
                case InventoryRoles.Viewer:
                    permissions.AddRange(new[]
                    {
                        InventoryPermissions.ReadItems,
                        InventoryPermissions.ReadLocations,
                        InventoryPermissions.ReadTransactions,
                        InventoryPermissions.ReadSuppliers,
                        InventoryPermissions.ViewReports
                    });
                    break;

                case InventoryRoles.Clerk:
                    permissions.AddRange(new[]
                    {
                        InventoryPermissions.ReadItems,
                        InventoryPermissions.CreateTransactions,
                        InventoryPermissions.ReadTransactions,
                        InventoryPermissions.UpdateTransactions,
                        InventoryPermissions.ReadLocations,
                        InventoryPermissions.ReadSuppliers
                    });
                    break;

                case InventoryRoles.Manager:
                    permissions.AddRange(new[]
                    {
                        InventoryPermissions.CreateItems,
                        InventoryPermissions.ReadItems,
                        InventoryPermissions.UpdateItems,
                        InventoryPermissions.CreateLocations,
                        InventoryPermissions.ReadLocations,
                        InventoryPermissions.UpdateLocations,
                        InventoryPermissions.CreateTransactions,
                        InventoryPermissions.ReadTransactions,
                        InventoryPermissions.UpdateTransactions,
                        InventoryPermissions.ApproveTransactions,
                        InventoryPermissions.CreateSuppliers,
                        InventoryPermissions.ReadSuppliers,
                        InventoryPermissions.UpdateSuppliers,
                        InventoryPermissions.ViewReports,
                        InventoryPermissions.ExportReports
                    });
                    break;

                case InventoryRoles.Admin:
                    permissions.AddRange(Enum.GetValues(typeof(InventoryPermissions))
                        .Cast<string>());
                    break;
            }
        }

        return permissions.Distinct();
    }
}

/// <summary>
/// Device Tracking user model (for integration)
/// </summary>
internal class DeviceTrackingUser
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
