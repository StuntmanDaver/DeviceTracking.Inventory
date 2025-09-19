using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Infrastructure.Data;
using DeviceTracking.Inventory.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceTracking.Inventory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for inventory item operations
/// </summary>
public class InventoryItemRepository : Repository<InventoryItem>, IInventoryItemRepository
{
    /// <summary>
    /// Constructor
    /// </summary>
    public InventoryItemRepository(InventoryDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets an inventory item by its unique identifier
    /// </summary>
    public override async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets an inventory item by its barcode
    /// </summary>
    public async Task<InventoryItem?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .FirstOrDefaultAsync(i => i.Barcode == barcode && i.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets an inventory item by its part number
    /// </summary>
    public async Task<InventoryItem?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .FirstOrDefaultAsync(i => i.PartNumber == partNumber && i.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all inventory items
    /// </summary>
    public override async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .Where(i => i.IsActive)
            .OrderBy(i => i.PartNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets inventory items with low stock levels
    /// </summary>
    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .Where(i => i.IsActive && i.CurrentStock <= threshold)
            .OrderBy(i => i.CurrentStock)
            .ThenBy(i => i.PartNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets inventory items by location
    /// </summary>
    public async Task<IEnumerable<InventoryItem>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .Where(i => i.IsActive && i.LocationId == locationId)
            .OrderBy(i => i.PartNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets inventory items by category
    /// </summary>
    public async Task<IEnumerable<InventoryItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .Where(i => i.IsActive && i.Category == category)
            .OrderBy(i => i.PartNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an inventory item exists by barcode
    /// </summary>
    public async Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .AnyAsync(i => i.Barcode == barcode, cancellationToken);
    }

    /// <summary>
    /// Checks if an inventory item exists by part number
    /// </summary>
    public async Task<bool> ExistsByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .AnyAsync(i => i.PartNumber == partNumber, cancellationToken);
    }

    /// <summary>
    /// Gets inventory items that need to be synced with QuickBooks
    /// </summary>
    public async Task<IEnumerable<InventoryItem>> GetItemsForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Location)
            .Include(i => i.Supplier)
            .Where(i => i.IsActive &&
                       (i.CreatedAt >= sinceDate || i.UpdatedAt >= sinceDate))
            .OrderBy(i => i.UpdatedAt ?? i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the last movement timestamp for an inventory item
    /// </summary>
    public async Task UpdateLastMovementAsync(Guid itemId, DateTime movementDate, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(itemId, cancellationToken);
        if (item != null)
        {
            item.LastMovement = movementDate;
            item.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(item, cancellationToken);
        }
    }

    /// <summary>
    /// Updates stock levels for an inventory item
    /// </summary>
    public async Task UpdateStockLevelAsync(Guid itemId, int quantityChange, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(itemId, cancellationToken);
        if (item != null)
        {
            item.CurrentStock += quantityChange;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            // Ensure stock doesn't go negative
            if (item.CurrentStock < 0)
            {
                item.CurrentStock = 0;
            }

            await UpdateAsync(item, cancellationToken);
        }
    }
}
