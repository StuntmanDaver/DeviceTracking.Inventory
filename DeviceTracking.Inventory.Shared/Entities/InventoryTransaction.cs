using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceTracking.Inventory.Shared.Entities;

/// <summary>
/// Represents a stock movement transaction in the inventory system
/// </summary>
[Table("InventoryTransactions", Schema = "Inventory")]
public class InventoryTransaction
{
    /// <summary>
    /// Unique identifier for the transaction
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Transaction number (auto-generated, unique)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of transaction
    /// </summary>
    [Required]
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Status of the transaction
    /// </summary>
    [Required]
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    /// <summary>
    /// ID of the inventory item being transacted
    /// </summary>
    [Required]
    public Guid InventoryItemId { get; set; }

    /// <summary>
    /// Navigation property for the inventory item
    /// </summary>
    [ForeignKey(nameof(InventoryItemId))]
    public virtual InventoryItem? InventoryItem { get; set; }

    /// <summary>
    /// ID of the source location (for transfers and issues)
    /// </summary>
    public Guid? SourceLocationId { get; set; }

    /// <summary>
    /// Navigation property for source location
    /// </summary>
    [ForeignKey(nameof(SourceLocationId))]
    public virtual Location? SourceLocation { get; set; }

    /// <summary>
    /// ID of the destination location (for transfers and receipts)
    /// </summary>
    public Guid? DestinationLocationId { get; set; }

    /// <summary>
    /// Navigation property for destination location
    /// </summary>
    [ForeignKey(nameof(DestinationLocationId))]
    public virtual Location? DestinationLocation { get; set; }

    /// <summary>
    /// Quantity involved in the transaction
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost at the time of transaction
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, double.MaxValue)]
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Total cost/value of the transaction
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [NotMapped]
    public decimal TotalCost => (UnitCost ?? 0) * Quantity;

    /// <summary>
    /// Reference number from external system (e.g., PO number, Order number)
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Type of reference (Purchase Order, Sales Order, etc.)
    /// </summary>
    [MaxLength(20)]
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Reason for stock adjustment (for adjustment transactions)
    /// </summary>
    [MaxLength(100)]
    public string? AdjustmentReason { get; set; }

    /// <summary>
    /// User who initiated the transaction
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InitiatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the transaction was initiated
    /// </summary>
    [Required]
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who approved the transaction (if required)
    /// </summary>
    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date and time when the transaction was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// User who completed/processed the transaction
    /// </summary>
    [MaxLength(100)]
    public string? ProcessedBy { get; set; }

    /// <summary>
    /// Date and time when the transaction was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Notes or comments about the transaction
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Before image of stock levels (for audit trail)
    /// </summary>
    [MaxLength(1000)]
    public string? BeforeImage { get; set; }

    /// <summary>
    /// After image of stock levels (for audit trail)
    /// </summary>
    [MaxLength(1000)]
    public string? AfterImage { get; set; }

    /// <summary>
    /// Whether this transaction has been synchronized with QuickBooks
    /// </summary>
    public bool IsQuickBooksSynced { get; set; } = false;

    /// <summary>
    /// QuickBooks transaction reference ID
    /// </summary>
    [MaxLength(50)]
    public string? QuickBooksRefId { get; set; }
}

/// <summary>
/// Enumeration of transaction types
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Stock receipt (incoming inventory)
    /// </summary>
    Receipt = 0,

    /// <summary>
    /// Stock issue (outgoing inventory)
    /// </summary>
    Issue = 1,

    /// <summary>
    /// Stock transfer between locations
    /// </summary>
    Transfer = 2,

    /// <summary>
    /// Stock adjustment (manual correction)
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Stock count adjustment (from physical inventory)
    /// </summary>
    CountAdjustment = 4
}

/// <summary>
/// Enumeration of transaction statuses
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction is pending approval or processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction has been approved
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Transaction is being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Transaction has been completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Transaction has been cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Transaction failed to process
    /// </summary>
    Failed = 5
}
