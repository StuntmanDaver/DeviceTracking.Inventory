using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Repositories;

/// <summary>
/// Repository interface for location operations
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Gets a location by its unique identifier
    /// </summary>
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a location by its code
    /// </summary>
    Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all locations
    /// </summary>
    Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active locations
    /// </summary>
    Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child locations for a parent location
    /// </summary>
    Task<IEnumerable<Location>> GetChildLocationsAsync(Guid parentLocationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations by type
    /// </summary>
    Task<IEnumerable<Location>> GetByTypeAsync(LocationType locationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a location exists by code
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new location
    /// </summary>
    Task AddAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing location
    /// </summary>
    Task UpdateAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a location by its identifier
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the hierarchy path for a location
    /// </summary>
    Task<string> GetHierarchyPathAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations with their current capacity utilization
    /// </summary>
    Task<IEnumerable<(Location Location, int ItemCount, decimal UtilizationPercent)>> GetCapacityUtilizationAsync(CancellationToken cancellationToken = default);
}
