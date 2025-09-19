using System;
using System.Collections.Generic;
using System.Linq;
using DeviceTracking.Inventory.Application.Common;

namespace DeviceTracking.Inventory.Application.Services.Authentication;

/// <summary>
/// Service for mapping Device Tracking roles to Inventory roles
/// </summary>
public class RoleMappingService
{
    private readonly Dictionary<string, InventoryRoleMapping> _roleMappings;

    public RoleMappingService()
    {
        _roleMappings = InitializeRoleMappings();
    }

    /// <summary>
    /// Map Device Tracking roles to Inventory roles
    /// </summary>
    public ServiceResult<IEnumerable<string>> MapRoles(IEnumerable<string> deviceTrackingRoles)
    {
        if (deviceTrackingRoles == null)
        {
            return ServiceResult<IEnumerable<string>>.Failure("Device Tracking roles cannot be null");
        }

        var inventoryRoles = new List<string>();

        foreach (var dtRole in deviceTrackingRoles)
        {
            if (_roleMappings.TryGetValue(dtRole, out var mapping))
            {
                inventoryRoles.AddRange(mapping.InventoryRoles);
            }
            else
            {
                // If no mapping exists, use the role as-is (for custom roles)
                inventoryRoles.Add(dtRole);
            }
        }

        // Remove duplicates and return
        return ServiceResult<IEnumerable<string>>.Success(inventoryRoles.Distinct());
    }

    /// <summary>
    /// Get permissions for inventory roles
    /// </summary>
    public ServiceResult<IEnumerable<string>> GetPermissionsForRoles(IEnumerable<string> inventoryRoles)
    {
        if (inventoryRoles == null)
        {
            return ServiceResult<IEnumerable<string>>.Failure("Inventory roles cannot be null");
        }

        var permissions = new List<string>();

        foreach (var role in inventoryRoles)
        {
            if (_roleMappings.Values.Any(mapping => mapping.InventoryRoles.Contains(role)))
            {
                var mapping = _roleMappings.Values.First(m => m.InventoryRoles.Contains(role));
                permissions.AddRange(mapping.Permissions);
            }
            else
            {
                // Handle custom roles - could be extended to look up from database
                permissions.AddRange(GetDefaultPermissionsForCustomRole(role));
            }
        }

        return ServiceResult<IEnumerable<string>>.Success(permissions.Distinct());
    }

    /// <summary>
    /// Validate role hierarchy
    /// </summary>
    public ServiceResult ValidateRoleHierarchy(string parentRole, string childRole)
    {
        if (string.IsNullOrEmpty(parentRole) || string.IsNullOrEmpty(childRole))
        {
            return ServiceResult.Failure("Roles cannot be empty");
        }

        // Define role hierarchy (higher numbers = more permissions)
        var roleHierarchy = new Dictionary<string, int>
        {
            [InventoryRoles.Viewer] = 1,
            [InventoryRoles.Clerk] = 2,
            [InventoryRoles.Manager] = 3,
            [InventoryRoles.Admin] = 4
        };

        if (!roleHierarchy.TryGetValue(parentRole, out var parentLevel))
        {
            return ServiceResult.Failure($"Unknown parent role: {parentRole}");
        }

        if (!roleHierarchy.TryGetValue(childRole, out var childLevel))
        {
            return ServiceResult.Failure($"Unknown child role: {childRole}");
        }

        if (parentLevel < childLevel)
        {
            return ServiceResult.Failure($"Cannot assign higher permission role '{childRole}' to user with role '{parentRole}'");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Get all available inventory roles
    /// </summary>
    public IEnumerable<string> GetAvailableRoles()
    {
        return new[]
        {
            InventoryRoles.Viewer,
            InventoryRoles.Clerk,
            InventoryRoles.Manager,
            InventoryRoles.Admin
        };
    }

    /// <summary>
    /// Get role description
    /// </summary>
    public string GetRoleDescription(string role)
    {
        return role switch
        {
            InventoryRoles.Viewer => "Read-only access to inventory data and reports",
            InventoryRoles.Clerk => "Basic inventory operations (receipt, issue, transfer)",
            InventoryRoles.Manager => "Full inventory management including adjustments and supplier management",
            InventoryRoles.Admin => "System administration and configuration",
            _ => "Custom role"
        };
    }

    /// <summary>
    /// Check if user has any of the required roles
    /// </summary>
    public bool HasAnyRole(IEnumerable<string> userRoles, IEnumerable<string> requiredRoles)
    {
        return userRoles.Any(userRole => requiredRoles.Contains(userRole));
    }

    /// <summary>
    /// Check if user has all required roles
    /// </summary>
    public bool HasAllRoles(IEnumerable<string> userRoles, IEnumerable<string> requiredRoles)
    {
        return requiredRoles.All(requiredRole => userRoles.Contains(requiredRole));
    }

    private Dictionary<string, InventoryRoleMapping> InitializeRoleMappings()
    {
        return new Dictionary<string, InventoryRoleMapping>
        {
            ["DeviceTracking.Viewer"] = new InventoryRoleMapping
            {
                DeviceTrackingRole = "DeviceTracking.Viewer",
                InventoryRoles = new[] { InventoryRoles.Viewer },
                Permissions = new[]
                {
                    InventoryPermissions.ReadItems,
                    InventoryPermissions.ReadLocations,
                    InventoryPermissions.ReadTransactions,
                    InventoryPermissions.ReadSuppliers,
                    InventoryPermissions.ViewReports
                }
            },
            ["DeviceTracking.Operator"] = new InventoryRoleMapping
            {
                DeviceTrackingRole = "DeviceTracking.Operator",
                InventoryRoles = new[] { InventoryRoles.Clerk },
                Permissions = new[]
                {
                    InventoryPermissions.ReadItems,
                    InventoryPermissions.CreateTransactions,
                    InventoryPermissions.ReadTransactions,
                    InventoryPermissions.UpdateTransactions,
                    InventoryPermissions.ReadLocations,
                    InventoryPermissions.ReadSuppliers
                }
            },
            ["DeviceTracking.Manager"] = new InventoryRoleMapping
            {
                DeviceTrackingRole = "DeviceTracking.Manager",
                InventoryRoles = new[] { InventoryRoles.Manager },
                Permissions = new[]
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
                }
            },
            ["DeviceTracking.Admin"] = new InventoryRoleMapping
            {
                DeviceTrackingRole = "DeviceTracking.Admin",
                InventoryRoles = new[] { InventoryRoles.Admin },
                Permissions = new[]
                {
                    InventoryPermissions.CreateItems,
                    InventoryPermissions.ReadItems,
                    InventoryPermissions.UpdateItems,
                    InventoryPermissions.DeleteItems,
                    InventoryPermissions.CreateLocations,
                    InventoryPermissions.ReadLocations,
                    InventoryPermissions.UpdateLocations,
                    InventoryPermissions.DeleteLocations,
                    InventoryPermissions.CreateTransactions,
                    InventoryPermissions.ReadTransactions,
                    InventoryPermissions.UpdateTransactions,
                    InventoryPermissions.ApproveTransactions,
                    InventoryPermissions.CreateSuppliers,
                    InventoryPermissions.ReadSuppliers,
                    InventoryPermissions.UpdateSuppliers,
                    InventoryPermissions.DeleteSuppliers,
                    InventoryPermissions.ViewReports,
                    InventoryPermissions.ExportReports,
                    InventoryPermissions.ManageUsers,
                    InventoryPermissions.ManageSettings,
                    InventoryPermissions.ViewAuditLogs
                }
            },
            ["DeviceTracking.SuperAdmin"] = new InventoryRoleMapping
            {
                DeviceTrackingRole = "DeviceTracking.SuperAdmin",
                InventoryRoles = new[] { InventoryRoles.Admin },
                Permissions = GetAllPermissions()
            }
        };
    }

    private IEnumerable<string> GetAllPermissions()
    {
        return Enum.GetValues(typeof(InventoryPermissions))
            .Cast<InventoryPermissions>()
            .Select(p => p.ToString());
    }

    private IEnumerable<string> GetDefaultPermissionsForCustomRole(string role)
    {
        // For custom roles, provide basic read permissions
        // In a production system, this would look up from a database
        return new[]
        {
            InventoryPermissions.ReadItems,
            InventoryPermissions.ReadLocations,
            InventoryPermissions.ReadTransactions,
            InventoryPermissions.ReadSuppliers
        };
    }
}

/// <summary>
/// Role mapping configuration
/// </summary>
public class InventoryRoleMapping
{
    /// <summary>
    /// Device Tracking role name
    /// </summary>
    public string DeviceTrackingRole { get; set; } = string.Empty;

    /// <summary>
    /// Corresponding inventory roles
    /// </summary>
    public IEnumerable<string> InventoryRoles { get; set; } = new List<string>();

    /// <summary>
    /// Permissions granted by this role mapping
    /// </summary>
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}

/// <summary>
/// Permission matrix for easy reference
/// </summary>
public static class PermissionMatrix
{
    /// <summary>
    /// Get permissions required for an operation
    /// </summary>
    public static IEnumerable<string> GetRequiredPermissions(string operation)
    {
        return operation switch
        {
            "CreateItem" => new[] { InventoryPermissions.CreateItems },
            "ReadItem" => new[] { InventoryPermissions.ReadItems },
            "UpdateItem" => new[] { InventoryPermissions.UpdateItems },
            "DeleteItem" => new[] { InventoryPermissions.DeleteItems },
            "CreateLocation" => new[] { InventoryPermissions.CreateLocations },
            "ReadLocation" => new[] { InventoryPermissions.ReadLocations },
            "UpdateLocation" => new[] { InventoryPermissions.UpdateLocations },
            "DeleteLocation" => new[] { InventoryPermissions.DeleteLocations },
            "CreateTransaction" => new[] { InventoryPermissions.CreateTransactions },
            "ReadTransaction" => new[] { InventoryPermissions.ReadTransactions },
            "UpdateTransaction" => new[] { InventoryPermissions.UpdateTransactions },
            "ApproveTransaction" => new[] { InventoryPermissions.ApproveTransactions },
            "CreateSupplier" => new[] { InventoryPermissions.CreateSuppliers },
            "ReadSupplier" => new[] { InventoryPermissions.ReadSuppliers },
            "UpdateSupplier" => new[] { InventoryPermissions.UpdateSuppliers },
            "DeleteSupplier" => new[] { InventoryPermissions.DeleteSuppliers },
            "ViewReports" => new[] { InventoryPermissions.ViewReports },
            "ExportReports" => new[] { InventoryPermissions.ExportReports },
            "ManageUsers" => new[] { InventoryPermissions.ManageUsers },
            "ManageSettings" => new[] { InventoryPermissions.ManageSettings },
            "ViewAuditLogs" => new[] { InventoryPermissions.ViewAuditLogs },
            _ => new[] { InventoryPermissions.ReadItems } // Default to read-only
        };
    }

    /// <summary>
    /// Check if user has permission for operation
    /// </summary>
    public static bool HasPermission(IEnumerable<string> userPermissions, string operation)
    {
        var requiredPermissions = GetRequiredPermissions(operation);
        return requiredPermissions.Any(required => userPermissions.Contains(required));
    }
}
