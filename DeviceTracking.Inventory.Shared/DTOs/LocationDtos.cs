using System;
using System.ComponentModel.DataAnnotations;

namespace DeviceTracking.Inventory.Shared.DTOs;

/// <summary>
/// DTO for creating a new location
/// </summary>
public class CreateLocationDto
{
    /// <summary>
    /// Location code or identifier (e.g., "WH-001", "PROD-A")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the location
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the location
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of location
    /// </summary>
    [Required]
    public string LocationType { get; set; } = "Warehouse";

    /// <summary>
    /// Parent location ID for hierarchical structure
    /// </summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    [MaxLength(100)]
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [MaxLength(50)]
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(50)]
    public string? Country { get; set; }

    /// <summary>
    /// Contact person for this location
    /// </summary>
    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Maximum capacity of the location (in units)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Whether this location is currently active
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing location
/// </summary>
public class UpdateLocationDto
{
    /// <summary>
    /// Human-readable name of the location
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Description of the location
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of location
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    [MaxLength(100)]
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [MaxLength(50)]
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(50)]
    public string? Country { get; set; }

    /// <summary>
    /// Contact person for this location
    /// </summary>
    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Maximum capacity of the location (in units)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Whether this location is currently active
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for location response (read operations)
/// </summary>
public class LocationDto
{
    /// <summary>
    /// Unique identifier for the location
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Location code or identifier (e.g., "WH-001", "PROD-A")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the location
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the location
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of location
    /// </summary>
    public string LocationType { get; set; } = "Warehouse";

    /// <summary>
    /// Parent location information
    /// </summary>
    public LocationSummaryDto? ParentLocation { get; set; }

    /// <summary>
    /// Child locations count
    /// </summary>
    public int ChildLocationsCount { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Full address formatted string
    /// </summary>
    public string FullAddress
    {
        get
        {
            var addressParts = new[]
            {
                Address,
                City,
                State,
                PostalCode,
                Country
            }.Where(part => !string.IsNullOrEmpty(part));

            return string.Join(", ", addressParts);
        }
    }

    /// <summary>
    /// Contact person for this location
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Maximum capacity of the location (in units)
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Current capacity utilization percentage
    /// </summary>
    public decimal CapacityUtilization { get; set; }

    /// <summary>
    /// Whether this location is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when this location was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created this location
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this location
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Total number of inventory items at this location
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total value of inventory at this location
    /// </summary>
    public decimal TotalValue { get; set; }
}

/// <summary>
/// DTO for location summary (used in other DTOs)
/// </summary>
public class LocationSummaryDto
{
    /// <summary>
    /// Unique identifier for the location
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Location code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the location
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of location
    /// </summary>
    public string LocationType { get; set; } = "Warehouse";

    /// <summary>
    /// Whether this location is currently active
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for location hierarchy information
/// </summary>
public class LocationHierarchyDto
{
    /// <summary>
    /// Unique identifier for the location
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Location code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the location
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of location
    /// </summary>
    public string LocationType { get; set; } = "Warehouse";

    /// <summary>
    /// Hierarchy level (0 = root, 1 = child, etc.)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Full hierarchy path (e.g., "Warehouse A > Aisle 1 > Shelf 2")
    /// </summary>
    public string HierarchyPath { get; set; } = string.Empty;

    /// <summary>
    /// Child locations
    /// </summary>
    public List<LocationHierarchyDto> Children { get; set; } = new();
}
