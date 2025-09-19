using System;
using System.ComponentModel.DataAnnotations;

namespace DeviceTracking.Inventory.Shared.DTOs;

/// <summary>
/// DTO for creating a new inventory item
/// </summary>
public class CreateInventoryItemDto
{
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
    [MaxLength(20)]
    public string? UnitOfMeasure { get; set; } = "Each";

    /// <summary>
    /// Current quantity in stock
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? CurrentStock { get; set; } = 0;

    /// <summary>
    /// Minimum stock level before reorder is recommended
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MinimumStock { get; set; } = 0;

    /// <summary>
    /// Maximum stock level for space/capacity planning
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MaximumStock { get; set; } = 0;

    /// <summary>
    /// Standard cost per unit
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? StandardCost { get; set; } = 0;

    /// <summary>
    /// Selling price per unit
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; } = 0;

    /// <summary>
    /// ID of the primary location where this item is stored
    /// </summary>
    [Required]
    public Guid LocationId { get; set; }

    /// <summary>
    /// ID of the supplier for this item
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing inventory item
/// </summary>
public class UpdateInventoryItemDto
{
    /// <summary>
    /// Human-readable description of the item
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

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
    [MaxLength(20)]
    public string? UnitOfMeasure { get; set; }

    /// <summary>
    /// Minimum stock level before reorder is recommended
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MinimumStock { get; set; }

    /// <summary>
    /// Maximum stock level for space/capacity planning
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MaximumStock { get; set; }

    /// <summary>
    /// Standard cost per unit
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? StandardCost { get; set; }

    /// <summary>
    /// Selling price per unit
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; }

    /// <summary>
    /// ID of the primary location where this item is stored
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// ID of the supplier for this item
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for inventory item response (read operations)
/// </summary>
public class InventoryItemDto
{
    /// <summary>
    /// Unique identifier for the inventory item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Part number or SKU (Stock Keeping Unit)
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the item
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Barcode or QR code value (unique identifier)
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// Category classification
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Subcategory for more detailed classification
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Unit of measure (e.g., "Each", "Box", "Pound")
    /// </summary>
    public string UnitOfMeasure { get; set; } = "Each";

    /// <summary>
    /// Current quantity in stock
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Quantity reserved for pending orders
    /// </summary>
    public int ReservedStock { get; set; }

    /// <summary>
    /// Available stock (CurrentStock - ReservedStock)
    /// </summary>
    public int AvailableStock => CurrentStock - ReservedStock;

    /// <summary>
    /// Minimum stock level before reorder is recommended
    /// </summary>
    public int MinimumStock { get; set; }

    /// <summary>
    /// Maximum stock level for space/capacity planning
    /// </summary>
    public int MaximumStock { get; set; }

    /// <summary>
    /// Standard cost per unit
    /// </summary>
    public decimal StandardCost { get; set; }

    /// <summary>
    /// Selling price per unit
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Primary location information
    /// </summary>
    public LocationSummaryDto? Location { get; set; }

    /// <summary>
    /// Supplier information
    /// </summary>
    public SupplierSummaryDto? Supplier { get; set; }

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time of last stock movement
    /// </summary>
    public DateTime? LastMovement { get; set; }

    /// <summary>
    /// Date and time when this item was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created this item
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this item
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if the item is low on stock
    /// </summary>
    public bool IsLowStock => CurrentStock <= MinimumStock;

    /// <summary>
    /// Stock status indicator
    /// </summary>
    public string StockStatus
    {
        get
        {
            if (CurrentStock == 0) return "Out of Stock";
            if (CurrentStock <= MinimumStock) return "Low Stock";
            if (CurrentStock >= MaximumStock) return "Overstocked";
            return "In Stock";
        }
    }
}

/// <summary>
/// DTO for inventory item summary (used in lists)
/// </summary>
public class InventoryItemSummaryDto
{
    /// <summary>
    /// Unique identifier for the inventory item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Part number or SKU (Stock Keeping Unit)
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the item
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Barcode or QR code value
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// Category classification
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Current quantity in stock
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Available stock (CurrentStock - ReservedStock)
    /// </summary>
    public int AvailableStock { get; set; }

    /// <summary>
    /// Standard cost per unit
    /// </summary>
    public decimal StandardCost { get; set; }

    /// <summary>
    /// Selling price per unit
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Location name
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Stock status indicator
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;
}

/// <summary>
/// DTO for low stock alerts
/// </summary>
public class LowStockAlertDto
{
    /// <summary>
    /// Inventory item ID
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Part number
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current stock level
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Minimum stock level
    /// </summary>
    public int MinimumStock { get; set; }

    /// <summary>
    /// Stock deficit (MinimumStock - CurrentStock)
    /// </summary>
    public int StockDeficit => MinimumStock - CurrentStock;

    /// <summary>
    /// Location name
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Supplier name
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Urgency level of the alert
    /// </summary>
    public string UrgencyLevel
    {
        get
        {
            if (CurrentStock == 0) return "Critical";
            if (StockDeficit >= MinimumStock * 0.5) return "High";
            return "Medium";
        }
    }
}
