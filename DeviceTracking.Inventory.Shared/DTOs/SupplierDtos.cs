using System;
using System.ComponentModel.DataAnnotations;

namespace DeviceTracking.Inventory.Shared.DTOs;

/// <summary>
/// DTO for creating a new supplier
/// </summary>
public class CreateSupplierDto
{
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
    [Phone]
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
    [Phone]
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
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Supplier performance rating (1-5 scale)
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// Additional notes or comments about the supplier
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing supplier
/// </summary>
public class UpdateSupplierDto
{
    /// <summary>
    /// Supplier company name
    /// </summary>
    [MaxLength(100)]
    public string? CompanyName { get; set; }

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
    [Phone]
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
    [Phone]
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
    public string? Currency { get; set; }

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
    public bool? IsActive { get; set; }

    /// <summary>
    /// Supplier performance rating (1-5 scale)
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// Additional notes or comments about the supplier
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for supplier response (read operations)
/// </summary>
public class SupplierDto
{
    /// <summary>
    /// Unique identifier for the supplier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Supplier code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Supplier company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person name
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact person's title
    /// </summary>
    public string? ContactTitle { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Secondary phone number
    /// </summary>
    public string? Phone2 { get; set; }

    /// <summary>
    /// Secondary email address
    /// </summary>
    public string? Email2 { get; set; }

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
    /// Tax identification number
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Payment terms (e.g., "Net 30", "Net 60")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Default currency for transactions with this supplier
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Average lead time in days for orders from this supplier
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Minimum order quantity required
    /// </summary>
    public int? MinimumOrderQuantity { get; set; }

    /// <summary>
    /// Whether this supplier is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Supplier performance rating (1-5 scale)
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Rating description
    /// </summary>
    public string RatingDescription
    {
        get
        {
            return Rating switch
            {
                5 => "Excellent",
                4 => "Very Good",
                3 => "Good",
                2 => "Fair",
                1 => "Poor",
                _ => "Not Rated"
            };
        }
    }

    /// <summary>
    /// Date and time when this supplier was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created this supplier
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated this supplier
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional notes or comments about the supplier
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Total number of inventory items supplied by this supplier
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total value of inventory supplied by this supplier
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Last order date from this supplier
    /// </summary>
    public DateTime? LastOrderDate { get; set; }
}

/// <summary>
/// DTO for supplier summary (used in other DTOs)
/// </summary>
public class SupplierSummaryDto
{
    /// <summary>
    /// Unique identifier for the supplier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Supplier code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Supplier company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person name
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Whether this supplier is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Supplier performance rating
    /// </summary>
    public int? Rating { get; set; }
}

/// <summary>
/// DTO for supplier performance metrics
/// </summary>
public class SupplierPerformanceDto
{
    /// <summary>
    /// Supplier ID
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Supplier name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of orders from this supplier
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Average lead time in days
    /// </summary>
    public decimal AverageLeadTime { get; set; }

    /// <summary>
    /// On-time delivery rate (percentage)
    /// </summary>
    public decimal OnTimeDeliveryRate { get; set; }

    /// <summary>
    /// Total value of orders
    /// </summary>
    public decimal TotalOrderValue { get; set; }

    /// <summary>
    /// Supplier rating
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Performance score (calculated)
    /// </summary>
    public decimal PerformanceScore
    {
        get
        {
            // Calculate performance score based on multiple factors
            var leadTimeScore = AverageLeadTime <= 7 ? 1.0m : AverageLeadTime <= 14 ? 0.8m : 0.6m;
            var deliveryScore = OnTimeDeliveryRate / 100;
            var ratingScore = Rating.HasValue ? (decimal)Rating.Value / 5 : 0.5m;

            return (leadTimeScore + deliveryScore + ratingScore) / 3;
        }
    }
}
