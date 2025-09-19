using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Repositories;

/// <summary>
/// Repository interface for inventory item operations
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Gets an inventory item by its unique identifier
    /// </summary>
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an inventory item by its barcode
    /// </summary>
    Task<InventoryItem?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an inventory item by its part number
    /// </summary>
    Task<InventoryItem?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory items with low stock levels
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory items by location
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory items by category
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an inventory item exists by barcode
    /// </summary>
    Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an inventory item exists by part number
    /// </summary>
    Task<bool> ExistsByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new inventory item
    /// </summary>
    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing inventory item
    /// </summary>
    Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an inventory item by its identifier
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory items that need to be synced with QuickBooks
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetItemsForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last movement timestamp for an inventory item
    /// </summary>
    Task UpdateLastMovementAsync(Guid itemId, DateTime movementDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates stock levels for an inventory item
    /// </summary>
    Task UpdateStockLevelAsync(Guid itemId, int quantityChange, CancellationToken cancellationToken = default);
}
