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
/// Implementation of inventory item service
/// </summary>
public class InventoryItemService : IInventoryItemService
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryItemService(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<InventoryItemDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByIdAsync(id, cancellationToken);
            if (item == null)
            {
                return ApiResponse<InventoryItemDto>.Fail("Inventory item not found");
            }

            var dto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error retrieving inventory item: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryItemDto>> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByBarcodeAsync(barcode, cancellationToken);
            if (item == null)
            {
                return ApiResponse<InventoryItemDto>.Fail("Inventory item not found for the specified barcode");
            }

            var dto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error retrieving inventory item by barcode: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<InventoryItemSummaryDto>>> GetPagedAsync(InventoryItemQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply filters
            var filter = BuildFilter(query);
            var items = await _inventoryItemRepository.FindAsync(filter, cancellationToken);
            var totalCount = await _inventoryItemRepository.CountAsync(filter, cancellationToken);

            // Apply sorting
            var sortedItems = ApplySorting(items, query.SortBy, query.SortDirection);

            // Apply pagination
            var pagedItems = sortedItems
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var dtos = _mapper.Map<IEnumerable<InventoryItemSummaryDto>>(pagedItems);

            var response = new PagedResponse<InventoryItemSummaryDto>
            {
                Items = dtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PagedResponse<InventoryItemSummaryDto>>.Ok(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<InventoryItemSummaryDto>>.Fail($"Error retrieving inventory items: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<LowStockAlertDto>>> GetLowStockAlertsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var lowStockItems = await _inventoryItemRepository.GetLowStockItemsAsync(threshold, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<LowStockAlertDto>>(lowStockItems);

            return ApiResponse<IEnumerable<LowStockAlertDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<LowStockAlertDto>>.Fail($"Error retrieving low stock alerts: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryItemDto>> CreateAsync(CreateInventoryItemDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create new inventory item
            var item = _mapper.Map<InventoryItem>(dto);
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            item.CreatedBy = userId;
            item.UpdatedBy = userId;

            await _inventoryItemRepository.AddAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(resultDto, "Inventory item created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error creating inventory item: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryItemDto>> UpdateAsync(Guid id, UpdateInventoryItemDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByIdAsync(id, cancellationToken);
            if (item == null)
            {
                return ApiResponse<InventoryItemDto>.Fail("Inventory item not found");
            }

            // Apply updates
            _mapper.Map(dto, item);
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            _inventoryItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(resultDto, "Inventory item updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error updating inventory item: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByIdAsync(id, cancellationToken);
            if (item == null)
            {
                return ApiResponse.Fail("Inventory item not found");
            }

            // Soft delete - mark as inactive
            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            _inventoryItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse.Ok("Inventory item deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error deleting inventory item: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryItemDto>> UpdateStockAsync(Guid id, int quantityChange, string reason, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByIdAsync(id, cancellationToken);
            if (item == null)
            {
                return ApiResponse<InventoryItemDto>.Fail("Inventory item not found");
            }

            // Update stock level
            var newStockLevel = item.CurrentStock + quantityChange;

            // Validate stock level
            if (newStockLevel < 0)
            {
                return ApiResponse<InventoryItemDto>.Fail("Stock level cannot be negative");
            }

            item.CurrentStock = newStockLevel;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            _inventoryItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(resultDto, $"Stock updated by {quantityChange}. New stock level: {newStockLevel}");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error updating stock: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryItemDto>> RecordBarcodeScanAsync(string barcode, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _inventoryItemRepository.GetByBarcodeAsync(barcode, cancellationToken);
            if (item == null)
            {
                return ApiResponse<InventoryItemDto>.Fail("Inventory item not found for the specified barcode");
            }

            // Update last movement timestamp
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            _inventoryItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = _mapper.Map<InventoryItemDto>(item);
            return ApiResponse<InventoryItemDto>.Ok(resultDto, "Barcode scan recorded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Error recording barcode scan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryItemDto>>> GetForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get items modified since the specified date
            var items = await _inventoryItemRepository.FindAsync(
                i => i.UpdatedAt >= sinceDate && i.IsActive,
                cancellationToken);

            var dtos = _mapper.Map<IEnumerable<InventoryItemDto>>(items);
            return ApiResponse<IEnumerable<InventoryItemDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<InventoryItemDto>>.Fail($"Error retrieving items for QuickBooks sync: {ex.Message}");
        }
    }

    public async Task<ApiResponse> BulkUpdateAsync(IEnumerable<(Guid Id, UpdateInventoryItemDto Dto)> updates, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var (id, dto) in updates)
            {
                var item = await _inventoryItemRepository.GetByIdAsync(id, cancellationToken);
                if (item == null)
                {
                    return ApiResponse.Fail($"Inventory item with ID {id} not found");
                }

                _mapper.Map(dto, item);
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = userId;

                _inventoryItemRepository.Update(item);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse.Ok($"{updates.Count()} inventory items updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error performing bulk update: {ex.Message}");
        }
    }

    public async Task<ApiResponse<(decimal TotalValue, int TotalItems, decimal AverageCost)>> GetInventoryValuationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _inventoryItemRepository.GetAllAsync(cancellationToken);
            var activeItems = items.Where(i => i.IsActive);

            var totalItems = activeItems.Count();
            var totalValue = activeItems.Sum(i => i.CurrentStock * i.StandardCost);
            var averageCost = totalItems > 0 ? totalValue / totalItems : 0;

            return ApiResponse<(decimal, int, decimal)>.Ok((totalValue, totalItems, averageCost));
        }
        catch (Exception ex)
        {
            return ApiResponse<(decimal, int, decimal)>.Fail($"Error calculating inventory valuation: {ex.Message}");
        }
    }

    private System.Linq.Expressions.Expression<Func<InventoryItem, bool>> BuildFilter(InventoryItemQueryDto query)
    {
        return item =>
            (!query.IsActive.HasValue || item.IsActive == query.IsActive.Value) &&
            (string.IsNullOrEmpty(query.Category) || item.Category == query.Category) &&
            (string.IsNullOrEmpty(query.SubCategory) || item.SubCategory == query.SubCategory) &&
            (!query.LocationId.HasValue || item.LocationId == query.LocationId.Value) &&
            (!query.SupplierId.HasValue || item.SupplierId == query.SupplierId.Value) &&
            (!query.MinStock.HasValue || item.CurrentStock >= query.MinStock.Value) &&
            (!query.MaxStock.HasValue || item.CurrentStock <= query.MaxStock.Value) &&
            (!query.MinCost.HasValue || item.StandardCost >= query.MinCost.Value) &&
            (!query.MaxCost.HasValue || item.StandardCost <= query.MaxCost.Value) &&
            (query.IncludeInactive || item.IsActive) &&
            (string.IsNullOrEmpty(query.SearchTerm) ||
             item.PartNumber.Contains(query.SearchTerm) ||
             item.Description.Contains(query.SearchTerm) ||
             item.Barcode.Contains(query.SearchTerm));
    }

    private IEnumerable<InventoryItem> ApplySorting(IEnumerable<InventoryItem> items, string? sortBy, SortDirection sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.CreatedAt)
                : items.OrderBy(i => i.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "partnumber" => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.PartNumber)
                : items.OrderBy(i => i.PartNumber),
            "description" => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.Description)
                : items.OrderBy(i => i.Description),
            "currentstock" => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.CurrentStock)
                : items.OrderBy(i => i.CurrentStock),
            "standardcost" => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.StandardCost)
                : items.OrderBy(i => i.StandardCost),
            "lastmovement" => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.LastMovement)
                : items.OrderBy(i => i.LastMovement),
            _ => sortDirection == SortDirection.Descending
                ? items.OrderByDescending(i => i.CreatedAt)
                : items.OrderBy(i => i.CreatedAt)
        };
    }
}
