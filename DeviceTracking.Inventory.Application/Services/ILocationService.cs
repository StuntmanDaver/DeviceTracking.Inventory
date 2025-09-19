using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.DTOs;

namespace DeviceTracking.Inventory.Application.Services;

/// <summary>
/// Service interface for location operations
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Get a location by ID
    /// </summary>
    Task<ApiResponse<LocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a location by code
    /// </summary>
    Task<ApiResponse<LocationDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get locations with pagination and filtering
    /// </summary>
    Task<ApiResponse<PagedResponse<LocationDto>>> GetPagedAsync(LocationQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get location hierarchy
    /// </summary>
    Task<ApiResponse<IEnumerable<LocationHierarchyDto>>> GetHierarchyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new location
    /// </summary>
    Task<ApiResponse<LocationDto>> CreateAsync(CreateLocationDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing location
    /// </summary>
    Task<ApiResponse<LocationDto>> UpdateAsync(Guid id, UpdateLocationDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a location
    /// </summary>
    Task<ApiResponse> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get capacity utilization for all locations
    /// </summary>
    Task<ApiResponse<IEnumerable<(LocationDto Location, int ItemCount, decimal UtilizationPercent)>>> GetCapacityUtilizationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get locations by type
    /// </summary>
    Task<ApiResponse<IEnumerable<LocationDto>>> GetByTypeAsync(string locationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move inventory items between locations
    /// </summary>
    Task<ApiResponse> TransferItemsAsync(Guid fromLocationId, Guid toLocationId, IEnumerable<(Guid ItemId, int Quantity)> items, string reason, string? userId = null, CancellationToken cancellationToken = default);
}
