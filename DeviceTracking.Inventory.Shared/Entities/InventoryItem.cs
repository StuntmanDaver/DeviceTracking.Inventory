using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceTracking.Inventory.Shared.Entities;

/// <summary>
/// Represents an inventory item in the system
/// </summary>
[Table("InventoryItems", Schema = "Inventory")]
public class InventoryItem
{
    /// <summary>
    /// Unique identifier for the inventory item
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Part number or SKU (Stock Keeping Unit)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the item
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Barcode or QR code value (unique identifier)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// Category classification
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Subcategory for more detailed classification
    /// </summary>
    [MaxLength(50)]
    public string? SubCategory { get; set; }

    /// <summary>
    /// Unit of measure (e.g., "Each", "Box", "Pound")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string UnitOfMeasure { get; set; } = "Each";

    /// <summary>
    /// Current quantity in stock
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CurrentStock { get; set; }

    /// <summary>
    /// Quantity reserved for pending orders
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ReservedStock { get; set; }

    /// <summary>
    /// Available stock (CurrentStock - ReservedStock)
    /// </summary>
    [NotMapped]
    public int AvailableStock => CurrentStock - ReservedStock;

    /// <summary>
    /// Minimum stock level before reorder is recommended
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MinimumStock { get; set; }

    /// <summary>
    /// Maximum stock level for space/capacity planning
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MaximumStock { get; set; }

    /// <summary>
    /// Standard cost per unit
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }

    /// <summary>
    /// Selling price per unit
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, double.MaxValue)]
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Primary location ID where this item is stored
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Foreign key navigation to location
    /// </summary>
    [ForeignKey(nameof(LocationId))]
    public virtual Location? Location { get; set; }

    /// <summary>
    /// Supplier ID for this item
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Foreign key navigation to supplier
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time of last stock movement
    /// </summary>
    public DateTime? LastMovement { get; set; }

    /// <summary>
    /// Date and time when this item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this item
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this item
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for transactions
    /// </summary>
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
