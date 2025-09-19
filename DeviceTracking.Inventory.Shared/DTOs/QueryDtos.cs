using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeviceTracking.Inventory.Shared.DTOs;

/// <summary>
/// Base query parameters for pagination and sorting
/// </summary>
public class BaseQueryDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Sort field name
    /// </summary>
    [MaxLength(50)]
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Search term for text-based filtering
    /// </summary>
    [MaxLength(100)]
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Sort direction enumeration
/// </summary>
public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

/// <summary>
/// Query parameters for inventory items
/// </summary>
public class InventoryItemQueryDto : BaseQueryDto
{
    /// <summary>
    /// Filter by category
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Filter by subcategory
    /// </summary>
    [MaxLength(50)]
    public string? SubCategory { get; set; }

    /// <summary>
    /// Filter by location ID
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Filter by supplier ID
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter for low stock items
    /// </summary>
    public bool? IsLowStock { get; set; }

    /// <summary>
    /// Filter by minimum stock level
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MinStock { get; set; }

    /// <summary>
    /// Filter by maximum stock level
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? MaxStock { get; set; }

    /// <summary>
    /// Filter by minimum cost
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinCost { get; set; }

    /// <summary>
    /// Filter by maximum cost
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxCost { get; set; }

    /// <summary>
    /// Include inactive items
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}

/// <summary>
/// Query parameters for locations
/// </summary>
public class LocationQueryDto : BaseQueryDto
{
    /// <summary>
    /// Filter by location type
    /// </summary>
    [MaxLength(20)]
    public string? LocationType { get; set; }

    /// <summary>
    /// Filter by parent location ID
    /// </summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by city
    /// </summary>
    [MaxLength(50)]
    public string? City { get; set; }

    /// <summary>
    /// Filter by state/province
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }

    /// <summary>
    /// Include hierarchy information
    /// </summary>
    public bool IncludeHierarchy { get; set; } = false;

    /// <summary>
    /// Include capacity information
    /// </summary>
    public bool IncludeCapacity { get; set; } = false;
}

/// <summary>
/// Query parameters for suppliers
/// </summary>
public class SupplierQueryDto : BaseQueryDto
{
    /// <summary>
    /// Filter by rating (minimum)
    /// </summary>
    [Range(1, 5)]
    public int? MinRating { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by city
    /// </summary>
    [MaxLength(50)]
    public string? City { get; set; }

    /// <summary>
    /// Filter by state/province
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }

    /// <summary>
    /// Filter by country
    /// </summary>
    [MaxLength(50)]
    public string? Country { get; set; }

    /// <summary>
    /// Include performance metrics
    /// </summary>
    public bool IncludePerformance { get; set; } = false;

    /// <summary>
    /// Filter by lead time (maximum days)
    /// </summary>
    [Range(0, 365)]
    public int? MaxLeadTime { get; set; }
}

/// <summary>
/// Query parameters for inventory transactions
/// </summary>
public class InventoryTransactionQueryDto : BaseQueryDto
{
    /// <summary>
    /// Filter by transaction type
    /// </summary>
    [MaxLength(20)]
    public string? TransactionType { get; set; }

    /// <summary>
    /// Filter by transaction status
    /// </summary>
    [MaxLength(20)]
    public string? Status { get; set; }

    /// <summary>
    /// Filter by inventory item ID
    /// </summary>
    public Guid? InventoryItemId { get; set; }

    /// <summary>
    /// Filter by location ID
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Filter by user who initiated the transaction
    /// </summary>
    [MaxLength(100)]
    public string? InitiatedBy { get; set; }

    /// <summary>
    /// Filter by date range - start date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter by date range - end date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by minimum quantity
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? MinQuantity { get; set; }

    /// <summary>
    /// Filter by maximum quantity
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? MaxQuantity { get; set; }

    /// <summary>
    /// Include audit trail information
    /// </summary>
    public bool IncludeAuditTrail { get; set; } = false;

    /// <summary>
    /// Filter for QuickBooks sync status
    /// </summary>
    public bool? IsQuickBooksSynced { get; set; }
}

/// <summary>
/// Generic paged response wrapper
/// </summary>
public class PagedResponse<T>
{
    /// <summary>
    /// The data items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// First item index on the current page (1-based)
    /// </summary>
    public int FirstItemIndex => ((Page - 1) * PageSize) + 1;

    /// <summary>
    /// Last item index on the current page
    /// </summary>
    public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);
}

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Create a failed response
    /// </summary>
    public static ApiResponse<T> Fail(string error, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Errors = errors
        };
    }
}

/// <summary>
/// API response without data (for operations that don't return data)
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Create a failed response
    /// </summary>
    public static ApiResponse Fail(string error, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = error,
            Errors = errors
        };
    }
}
