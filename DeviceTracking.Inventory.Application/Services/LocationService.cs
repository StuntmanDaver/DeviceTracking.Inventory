using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeviceTracking.Inventory.Application.Common;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Application.Services;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Services;

/// <summary>
/// Implementation of location service
/// </summary>
public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LocationService(
        ILocationRepository locationRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<LocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await _locationRepository.GetByIdAsync(id, cancellationToken);
            if (location == null)
            {
                return ApiResponse<LocationDto>.Fail("Location not found");
            }

            var dto = await MapLocationToDtoAsync(location, cancellationToken);
            return ApiResponse<LocationDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<LocationDto>.Fail($"Error retrieving location: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LocationDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await _locationRepository.GetByCodeAsync(code, cancellationToken);
            if (location == null)
            {
                return ApiResponse<LocationDto>.Fail("Location not found");
            }

            var dto = await MapLocationToDtoAsync(location, cancellationToken);
            return ApiResponse<LocationDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<LocationDto>.Fail($"Error retrieving location by code: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<LocationDto>>> GetPagedAsync(LocationQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply filters
            var filter = BuildFilter(query);
            var locations = await _locationRepository.FindAsync(filter, cancellationToken);
            var totalCount = await _locationRepository.CountAsync(filter, cancellationToken);

            // Apply sorting
            var sortedLocations = ApplySorting(locations, query.SortBy, query.SortDirection);

            // Apply pagination
            var pagedLocations = sortedLocations
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var dtos = new List<LocationDto>();
            foreach (var location in pagedLocations)
            {
                dtos.Add(await MapLocationToDtoAsync(location, cancellationToken));
            }

            var response = new PagedResponse<LocationDto>
            {
                Items = dtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PagedResponse<LocationDto>>.Ok(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<LocationDto>>.Fail($"Error retrieving locations: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LocationDto>> CreateAsync(CreateLocationDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create new location
            var location = _mapper.Map<Location>(dto);
            location.Id = Guid.NewGuid();
            location.CreatedAt = DateTime.UtcNow;
            location.UpdatedAt = DateTime.UtcNow;
            location.CreatedBy = userId;
            location.UpdatedBy = userId;

            await _locationRepository.AddAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapLocationToDtoAsync(location, cancellationToken);
            return ApiResponse<LocationDto>.Ok(resultDto, "Location created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<LocationDto>.Fail($"Error creating location: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LocationDto>> UpdateAsync(Guid id, UpdateLocationDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await _locationRepository.GetByIdAsync(id, cancellationToken);
            if (location == null)
            {
                return ApiResponse<LocationDto>.Fail("Location not found");
            }

            // Apply updates
            _mapper.Map(dto, location);
            location.UpdatedAt = DateTime.UtcNow;
            location.UpdatedBy = userId;

            _locationRepository.Update(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapLocationToDtoAsync(location, cancellationToken);
            return ApiResponse<LocationDto>.Ok(resultDto, "Location updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<LocationDto>.Fail($"Error updating location: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await _locationRepository.GetByIdAsync(id, cancellationToken);
            if (location == null)
            {
                return ApiResponse.Fail("Location not found");
            }

            // Check for inventory items at this location
            var items = await _inventoryItemRepository.GetByLocationAsync(id, cancellationToken);
            if (items.Any())
            {
                return ApiResponse.Fail("Cannot delete location that contains inventory items");
            }

            // Soft delete - mark as inactive
            location.IsActive = false;
            location.UpdatedAt = DateTime.UtcNow;
            location.UpdatedBy = userId;

            _locationRepository.Update(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse.Ok("Location deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error deleting location: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<LocationHierarchyDto>>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hierarchy = await BuildLocationHierarchyAsync(cancellationToken);
            return ApiResponse<IEnumerable<LocationHierarchyDto>>.Ok(hierarchy);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<LocationHierarchyDto>>.Fail($"Error retrieving location hierarchy: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<LocationDto>>> GetByTypeAsync(string locationType, CancellationToken cancellationToken = default)
    {
        try
        {
            var locations = await _locationRepository.FindAsync(
                l => l.LocationType.ToString() == locationType && l.IsActive,
                cancellationToken);

            var dtos = new List<LocationDto>();
            foreach (var location in locations)
            {
                dtos.Add(await MapLocationToDtoAsync(location, cancellationToken));
            }

            return ApiResponse<IEnumerable<LocationDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<LocationDto>>.Fail($"Error retrieving locations by type: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<(LocationDto Location, int ItemCount, decimal UtilizationPercent)>>> GetCapacityUtilizationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var locations = await _locationRepository.GetAllAsync(cancellationToken);
            var utilizationData = new List<(LocationDto, int, decimal)>();

            foreach (var location in locations.Where(l => l.IsActive))
            {
                var dto = await MapLocationToDtoAsync(location, cancellationToken);
                var itemCount = await GetLocationItemCountAsync(location.Id, cancellationToken);

                decimal utilizationPercent = 0;
                if (location.MaxCapacity.HasValue && location.MaxCapacity.Value > 0)
                {
                    utilizationPercent = (decimal)itemCount / location.MaxCapacity.Value * 100;
                }

                utilizationData.Add((dto, itemCount, utilizationPercent));
            }

            return ApiResponse<IEnumerable<(LocationDto, int, decimal)>>.Ok(utilizationData);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<(LocationDto, int, decimal)>>.Fail($"Error calculating capacity utilization: {ex.Message}");
        }
    }

    public async Task<ApiResponse> TransferItemsAsync(Guid fromLocationId, Guid toLocationId, IEnumerable<(Guid ItemId, int Quantity)> items, string reason, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var (itemId, quantity) in items)
            {
                var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
                if (item == null)
                {
                    return ApiResponse.Fail($"Inventory item with ID {itemId} not found");
                }

                if (item.LocationId != fromLocationId)
                {
                    return ApiResponse.Fail($"Item {item.PartNumber} is not at the source location");
                }

                if (item.CurrentStock < quantity)
                {
                    return ApiResponse.Fail($"Insufficient stock for item {item.PartNumber}. Available: {item.CurrentStock}, Requested: {quantity}");
                }

                // Update item location
                item.LocationId = toLocationId;
                item.CurrentStock -= quantity; // Reduce stock at source
                item.LastMovement = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = userId;

                _inventoryItemRepository.Update(item);

                // TODO: Create inventory transaction record for the transfer
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse.Ok($"Successfully transferred {items.Count()} items between locations");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error transferring items: {ex.Message}");
        }
    }

    private async Task<LocationDto> MapLocationToDtoAsync(Location location, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<LocationDto>(location);

        // Calculate additional properties
        dto.TotalItems = await GetLocationItemCountAsync(location.Id, cancellationToken);
        dto.TotalValue = await GetLocationTotalValueAsync(location.Id, cancellationToken);

        if (location.MaxCapacity.HasValue && location.MaxCapacity.Value > 0)
        {
            dto.CapacityUtilization = (decimal)dto.TotalItems / location.MaxCapacity.Value * 100;
        }

        return dto;
    }

    private async Task<int> GetLocationItemCountAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        return items.Count();
    }

    private async Task<decimal> GetLocationTotalValueAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        return items.Sum(i => i.CurrentStock * i.StandardCost);
    }

    private async Task<List<LocationHierarchyDto>> BuildLocationHierarchyAsync(CancellationToken cancellationToken)
    {
        var allLocations = await _locationRepository.GetAllAsync(cancellationToken);
        var locationDict = allLocations.ToDictionary(l => l.Id);

        // Find root locations
        var rootLocations = allLocations.Where(l => !l.ParentLocationId.HasValue);

        var hierarchy = new List<LocationHierarchyDto>();
        foreach (var root in rootLocations.Where(l => l.IsActive))
        {
            hierarchy.Add(await BuildHierarchyNodeAsync(root, locationDict, 0, cancellationToken));
        }

        return hierarchy;
    }

    private async Task<LocationHierarchyDto> BuildHierarchyNodeAsync(
        Location location,
        Dictionary<Guid, Location> locationDict,
        int level,
        CancellationToken cancellationToken)
    {
        var node = _mapper.Map<LocationHierarchyDto>(location);
        node.Level = level;
        node.HierarchyPath = await BuildHierarchyPathAsync(location, locationDict, cancellationToken);

        // Build children
        var children = locationDict.Values
            .Where(l => l.ParentLocationId == location.Id && l.IsActive)
            .OrderBy(l => l.Code);

        var childNodes = new List<LocationHierarchyDto>();
        foreach (var child in children)
        {
            childNodes.Add(await BuildHierarchyNodeAsync(child, locationDict, level + 1, cancellationToken));
        }

        node.Children = childNodes;
        return node;
    }

    private async Task<string> BuildHierarchyPathAsync(
        Location location,
        Dictionary<Guid, Location> locationDict,
        CancellationToken cancellationToken)
    {
        var pathParts = new List<string>();
        var current = location;

        while (current != null)
        {
            pathParts.Insert(0, $"{current.Code} ({current.Name})");
            if (current.ParentLocationId.HasValue && locationDict.TryGetValue(current.ParentLocationId.Value, out var parent))
            {
                current = parent;
            }
            else
            {
                current = null;
            }
        }

        return string.Join(" > ", pathParts);
    }

    private System.Linq.Expressions.Expression<Func<Location, bool>> BuildFilter(LocationQueryDto query)
    {
        return location =>
            (!query.IsActive.HasValue || location.IsActive == query.IsActive.Value) &&
            (string.IsNullOrEmpty(query.LocationType) || location.LocationType.ToString() == query.LocationType) &&
            (!query.ParentLocationId.HasValue || location.ParentLocationId == query.ParentLocationId.Value) &&
            (string.IsNullOrEmpty(query.City) || location.City == query.City) &&
            (string.IsNullOrEmpty(query.State) || location.State == query.State) &&
            (string.IsNullOrEmpty(query.SearchTerm) ||
             location.Code.Contains(query.SearchTerm) ||
             location.Name.Contains(query.SearchTerm) ||
             location.Description.Contains(query.SearchTerm));
    }

    private IEnumerable<Location> ApplySorting(IEnumerable<Location> locations, string? sortBy, SortDirection sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDirection == SortDirection.Descending
                ? locations.OrderByDescending(l => l.CreatedAt)
                : locations.OrderBy(l => l.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "code" => sortDirection == SortDirection.Descending
                ? locations.OrderByDescending(l => l.Code)
                : locations.OrderBy(l => l.Code),
            "name" => sortDirection == SortDirection.Descending
                ? locations.OrderByDescending(l => l.Name)
                : locations.OrderBy(l => l.Name),
            "type" => sortDirection == SortDirection.Descending
                ? locations.OrderByDescending(l => l.LocationType)
                : locations.OrderBy(l => l.LocationType),
            _ => sortDirection == SortDirection.Descending
                ? locations.OrderByDescending(l => l.CreatedAt)
                : locations.OrderBy(l => l.CreatedAt)
        };
    }
}
