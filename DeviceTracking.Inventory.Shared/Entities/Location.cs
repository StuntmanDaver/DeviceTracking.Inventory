using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceTracking.Inventory.Shared.Entities;

/// <summary>
/// Represents a physical location where inventory items are stored
/// </summary>
[Table("Locations", Schema = "Inventory")]
public class Location
{
    /// <summary>
    /// Unique identifier for the location
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

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
    public LocationType LocationType { get; set; } = LocationType.Warehouse;

    /// <summary>
    /// Parent location ID for hierarchical structure (e.g., warehouse > aisle > shelf)
    /// </summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>
    /// Navigation property for parent location
    /// </summary>
    [ForeignKey(nameof(ParentLocationId))]
    public virtual Location? ParentLocation { get; set; }

    /// <summary>
    /// Navigation property for child locations
    /// </summary>
    public virtual ICollection<Location> ChildLocations { get; set; } = new List<Location>();

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
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Maximum capacity of the location (in units)
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Whether this location is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when this location was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this location
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this location
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for inventory items stored in this location
    /// </summary>
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    /// <summary>
    /// Navigation property for transactions involving this location as source
    /// </summary>
    public virtual ICollection<InventoryTransaction> SourceTransactions { get; set; } = new List<InventoryTransaction>();

    /// <summary>
    /// Navigation property for transactions involving this location as destination
    /// </summary>
    public virtual ICollection<InventoryTransaction> DestinationTransactions { get; set; } = new List<InventoryTransaction>();
}

/// <summary>
/// Enumeration of location types
/// </summary>
public enum LocationType
{
    /// <summary>
    /// Warehouse storage location
    /// </summary>
    Warehouse = 0,

    /// <summary>
    /// Production floor location
    /// </summary>
    Production = 1,

    /// <summary>
    /// Customer site location
    /// </summary>
    Customer = 2,

    /// <summary>
    /// Supplier/vendor location
    /// </summary>
    Supplier = 3,

    /// <summary>
    /// Transit or in-transit location
    /// </summary>
    Transit = 4
}
