using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.BusinessRules;

/// <summary>
/// Business rules for location hierarchy management
/// </summary>
public class LocationHierarchyRules
{
    private readonly ILocationRepository _locationRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public LocationHierarchyRules(
        ILocationRepository locationRepository,
        IInventoryItemRepository inventoryItemRepository)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
    }

    /// <summary>
    /// Validate location hierarchy rules
    /// </summary>
    public async Task<ServiceResult> ValidateHierarchyAsync(Guid locationId, Guid? parentLocationId, CancellationToken cancellationToken = default)
    {
        // Prevent self-referencing
        if (parentLocationId == locationId)
        {
            return ServiceResult.Failure("Location cannot be its own parent");
        }

        if (!parentLocationId.HasValue)
        {
            return ServiceResult.Success(); // Root level is always valid
        }

        // Check if parent exists
        var parentLocation = await _locationRepository.GetByIdAsync(parentLocationId.Value, cancellationToken);
        if (parentLocation == null)
        {
            return ServiceResult.Failure("Parent location does not exist");
        }

        // Check for circular references
        var circularCheck = await DetectCircularReferenceAsync(locationId, parentLocationId.Value, cancellationToken);
        if (!circularCheck.IsSuccess)
        {
            return circularCheck;
        }

        // Validate hierarchy depth (prevent too deep nesting)
        var hierarchyDepth = await CalculateHierarchyDepthAsync(parentLocationId.Value, cancellationToken);
        if (hierarchyDepth >= 5) // Max depth of 5 levels
        {
            return ServiceResult.Failure("Maximum hierarchy depth (5 levels) would be exceeded");
        }

        // Validate location type compatibility
        var currentLocation = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        if (currentLocation != null)
        {
            var typeCompatibility = ValidateLocationTypeCompatibility(currentLocation.Type, parentLocation.Type);
            if (!typeCompatibility.IsSuccess)
            {
                return typeCompatibility;
            }
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Detect circular references in location hierarchy
    /// </summary>
    private async Task<ServiceResult> DetectCircularReferenceAsync(Guid locationId, Guid parentId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid> { locationId };
        var currentParentId = parentId;

        while (currentParentId != Guid.Empty)
        {
            if (visited.Contains(currentParentId))
            {
                return ServiceResult.Failure("Circular reference detected in location hierarchy");
            }

            visited.Add(currentParentId);

            var parent = await _locationRepository.GetByIdAsync(currentParentId, cancellationToken);
            if (parent == null)
            {
                break; // Parent doesn't exist, no circular reference possible
            }

            currentParentId = parent.ParentLocationId ?? Guid.Empty;
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Calculate hierarchy depth for a location
    /// </summary>
    private async Task<int> CalculateHierarchyDepthAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var depth = 0;
        var currentId = locationId;

        while (currentId != Guid.Empty)
        {
            var location = await _locationRepository.GetByIdAsync(currentId, cancellationToken);
            if (location == null || !location.ParentLocationId.HasValue)
            {
                break;
            }

            depth++;
            currentId = location.ParentLocationId.Value;

            // Prevent infinite loops
            if (depth > 10)
            {
                throw new InvalidOperationException("Hierarchy depth calculation exceeded maximum iterations");
            }
        }

        return depth;
    }

    /// <summary>
    /// Validate location type compatibility in hierarchy
    /// </summary>
    private ServiceResult ValidateLocationTypeCompatibility(LocationType childType, LocationType parentType)
    {
        // Define valid parent-child relationships
        var validRelationships = new Dictionary<LocationType, LocationType[]>
        {
            [LocationType.Warehouse] = new[] { LocationType.ProductionFloor, LocationType.CustomerSite },
            [LocationType.ProductionFloor] = new[] { LocationType.Warehouse, LocationType.CustomerSite },
            [LocationType.CustomerSite] = new[] { LocationType.Warehouse, LocationType.ProductionFloor },
            [LocationType.SupplierLocation] = Array.Empty<LocationType>(), // Cannot have children
            [LocationType.Transit] = Array.Empty<LocationType>(), // Cannot have children
            [LocationType.Quarantine] = Array.Empty<LocationType>(), // Cannot have children
            [LocationType.Other] = new[] { LocationType.Warehouse, LocationType.ProductionFloor, LocationType.CustomerSite, LocationType.SupplierLocation }
        };

        if (validRelationships.TryGetValue(parentType, out var allowedChildren))
        {
            if (!allowedChildren.Contains(childType))
            {
                return ServiceResult.Failure($"Location type '{childType}' is not compatible as a child of '{parentType}'");
            }
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Get complete location hierarchy
    /// </summary>
    public async Task<ServiceResult<IEnumerable<LocationHierarchyNode>>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var allLocations = await _locationRepository.GetAllAsync(cancellationToken);
        var locationDict = allLocations.ToDictionary(l => l.Id);

        // Find root locations (no parent)
        var rootLocations = allLocations.Where(l => !l.ParentLocationId.HasValue);

        var hierarchy = new List<LocationHierarchyNode>();

        foreach (var rootLocation in rootLocations)
        {
            hierarchy.Add(await BuildHierarchyNodeAsync(rootLocation, locationDict, 0, cancellationToken));
        }

        return ServiceResult<IEnumerable<LocationHierarchyNode>>.Success(hierarchy);
    }

    /// <summary>
    /// Build hierarchy node recursively
    /// </summary>
    private async Task<LocationHierarchyNode> BuildHierarchyNodeAsync(
        Location location,
        Dictionary<Guid, Location> locationDict,
        int level,
        CancellationToken cancellationToken)
    {
        var node = new LocationHierarchyNode
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name,
            Type = location.Type,
            Level = level,
            IsActive = location.IsActive,
            ChildCount = 0
        };

        // Get children
        var children = locationDict.Values
            .Where(l => l.ParentLocationId == location.Id)
            .OrderBy(l => l.Code);

        var childNodes = new List<LocationHierarchyNode>();
        foreach (var child in children)
        {
            childNodes.Add(await BuildHierarchyNodeAsync(child, locationDict, level + 1, cancellationToken));
        }

        node.Children = childNodes;
        node.ChildCount = childNodes.Count;

        // Calculate item count for this location
        node.ItemCount = await GetLocationItemCountAsync(location.Id, cancellationToken);

        return node;
    }

    /// <summary>
    /// Get item count for a location
    /// </summary>
    private async Task<int> GetLocationItemCountAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        return items.Count();
    }

    /// <summary>
    /// Validate location deletion
    /// </summary>
    public async Task<ServiceResult> ValidateDeletionAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        // Check for child locations
        var children = await _locationRepository.GetChildLocationsAsync(locationId, cancellationToken);
        if (children.Any())
        {
            return ServiceResult.Failure("Cannot delete location that has child locations");
        }

        // Check for inventory items
        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        if (items.Any())
        {
            return ServiceResult.Failure("Cannot delete location that contains inventory items");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Get all ancestor locations (parents up the hierarchy)
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Location>>> GetAncestorsAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var ancestors = new List<Location>();
        var currentId = locationId;

        while (currentId != Guid.Empty)
        {
            var location = await _locationRepository.GetByIdAsync(currentId, cancellationToken);
            if (location == null)
            {
                break;
            }

            if (!location.ParentLocationId.HasValue)
            {
                break; // Root reached
            }

            var parent = await _locationRepository.GetByIdAsync(location.ParentLocationId.Value, cancellationToken);
            if (parent == null)
            {
                break;
            }

            ancestors.Add(parent);
            currentId = parent.Id;
        }

        return ServiceResult<IEnumerable<Location>>.Success(ancestors);
    }

    /// <summary>
    /// Get all descendant locations (children down the hierarchy)
    /// </summary>
    public async Task<ServiceResult<IEnumerable<Location>>> GetDescendantsAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var descendants = new List<Location>();
        var toProcess = new Queue<Guid>();
        toProcess.Enqueue(locationId);

        while (toProcess.Count > 0)
        {
            var currentId = toProcess.Dequeue();
            var children = await _locationRepository.GetChildLocationsAsync(currentId, cancellationToken);

            foreach (var child in children)
            {
                descendants.Add(child);
                toProcess.Enqueue(child.Id);
            }
        }

        return ServiceResult<IEnumerable<Location>>.Success(descendants);
    }

    /// <summary>
    /// Calculate location capacity utilization
    /// </summary>
    public async Task<ServiceResult<decimal>> CalculateCapacityUtilizationAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        if (location == null)
        {
            return ServiceResult<decimal>.Failure("Location not found");
        }

        if (!location.MaxCapacity.HasValue || location.MaxCapacity.Value == 0)
        {
            return ServiceResult<decimal>.Success(0, "Location has no capacity limit set");
        }

        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        var totalQuantity = items.Sum(i => i.CurrentStock);

        var utilization = (decimal)totalQuantity / location.MaxCapacity.Value * 100;

        return ServiceResult<decimal>.Success(utilization);
    }
}

/// <summary>
/// Node in location hierarchy
/// </summary>
public class LocationHierarchyNode
{
    /// <summary>
    /// Location ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Location code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Location name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Location type
    /// </summary>
    public LocationType Type { get; set; }

    /// <summary>
    /// Hierarchy level (0 = root)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Whether location is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of child locations
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// Number of inventory items at this location
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Child locations
    /// </summary>
    public IEnumerable<LocationHierarchyNode> Children { get; set; } = new List<LocationHierarchyNode>();

    /// <summary>
    /// Full hierarchy path
    /// </summary>
    public string HierarchyPath
    {
        get
        {
            var path = new List<string>();
            BuildPath(this, path);
            return string.Join(" > ", path);
        }
    }

    private void BuildPath(LocationHierarchyNode node, List<string> path)
    {
        path.Insert(0, $"{node.Code} ({node.Name})");
        // Note: This is a simplified path building. In a full implementation,
        // you'd need to traverse up the hierarchy to build the complete path.
    }
}
