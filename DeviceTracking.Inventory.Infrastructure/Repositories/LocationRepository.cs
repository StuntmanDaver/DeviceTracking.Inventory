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
/// Repository implementation for location operations
/// </summary>
public class LocationRepository : Repository<Location>, ILocationRepository
{
    /// <summary>
    /// Constructor
    /// </summary>
    public LocationRepository(InventoryDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a location by its code
    /// </summary>
    public async Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Include(l => l.ParentLocation)
            .Include(l => l.ChildLocations)
            .FirstOrDefaultAsync(l => l.Code == code && l.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all active locations
    /// </summary>
    public async Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets child locations for a parent location
    /// </summary>
    public async Task<IEnumerable<Location>> GetChildLocationsAsync(Guid parentLocationId, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.ParentLocationId == parentLocationId && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets locations by type
    /// </summary>
    public async Task<IEnumerable<Location>> GetByTypeAsync(LocationType locationType, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.IsActive && l.LocationType == locationType)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a location exists by code
    /// </summary>
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .AnyAsync(l => l.Code == code, cancellationToken);
    }

    /// <summary>
    /// Gets the hierarchy path for a location
    /// </summary>
    public async Task<string> GetHierarchyPathAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        // Simplified implementation - in a real scenario, this would traverse the hierarchy
        var location = await GetByIdAsync(locationId, cancellationToken);
        return location?.Name ?? string.Empty;
    }

    /// <summary>
    /// Gets locations with their current capacity utilization
    /// </summary>
    public async Task<IEnumerable<(Location Location, int ItemCount, decimal UtilizationPercent)>> GetCapacityUtilizationAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _context.Locations
            .Where(l => l.IsActive && l.MaxCapacity.HasValue)
            .Include(l => l.InventoryItems)
            .ToListAsync(cancellationToken);

        return locations.Select(l =>
        {
            var itemCount = l.InventoryItems?.Count ?? 0;
            var utilizationPercent = l.MaxCapacity.HasValue && l.MaxCapacity.Value > 0
                ? (decimal)itemCount / l.MaxCapacity.Value * 100
                : 0;
            return (l, itemCount, utilizationPercent);
        });
    }
}
