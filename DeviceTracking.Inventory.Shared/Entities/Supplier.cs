using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceTracking.Inventory.Shared.Entities;

/// <summary>
/// Represents a supplier/vendor in the inventory system
/// </summary>
[Table("Suppliers", Schema = "Inventory")]
public class Supplier
{
    /// <summary>
    /// Unique identifier for the supplier
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Supplier code or identifier
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Supplier company name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person name
    /// </summary>
    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact person's title
    /// </summary>
    [MaxLength(50)]
    public string? ContactTitle { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Secondary phone number
    /// </summary>
    [MaxLength(20)]
    public string? Phone2 { get; set; }

    /// <summary>
    /// Secondary email address
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? Email2 { get; set; }

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
    /// Tax identification number
    /// </summary>
    [MaxLength(20)]
    public string? TaxId { get; set; }

    /// <summary>
    /// Payment terms (e.g., "Net 30", "Net 60")
    /// </summary>
    [MaxLength(50)]
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Default currency for transactions with this supplier
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; } = "USD";

    /// <summary>
    /// Average lead time in days for orders from this supplier
    /// </summary>
    [Range(0, 365)]
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Minimum order quantity required
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MinimumOrderQuantity { get; set; }

    /// <summary>
    /// Whether this supplier is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Supplier performance rating (1-5 scale)
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// Date and time when this supplier was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this supplier
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this supplier
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments about the supplier
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for inventory items supplied by this supplier
    /// </summary>
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
