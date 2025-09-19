using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.DTOs;

namespace DeviceTracking.Inventory.Application.Services;

/// <summary>
/// Service interface for inventory item operations
/// </summary>
public interface IInventoryItemService
{
    /// <summary>
    /// Get an inventory item by ID
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an inventory item by barcode
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory items with pagination and filtering
    /// </summary>
    Task<ApiResponse<PagedResponse<InventoryItemSummaryDto>>> GetPagedAsync(InventoryItemQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    Task<ApiResponse<IEnumerable<LowStockAlertDto>>> GetLowStockAlertsAsync(int threshold = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> CreateAsync(CreateInventoryItemDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing inventory item
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> UpdateAsync(Guid id, UpdateInventoryItemDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an inventory item
    /// </summary>
    Task<ApiResponse> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update stock levels for an inventory item
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> UpdateStockAsync(Guid id, int quantityChange, string reason, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record barcode scan for an inventory item
    /// </summary>
    Task<ApiResponse<InventoryItemDto>> RecordBarcodeScanAsync(string barcode, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory items for QuickBooks synchronization
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryItemDto>>> GetForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update inventory items
    /// </summary>
    Task<ApiResponse> BulkUpdateAsync(IEnumerable<(Guid Id, UpdateInventoryItemDto Dto)> updates, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory valuation report
    /// </summary>
    Task<ApiResponse<(decimal TotalValue, int TotalItems, decimal AverageCost)>> GetInventoryValuationAsync(CancellationToken cancellationToken = default);
}
