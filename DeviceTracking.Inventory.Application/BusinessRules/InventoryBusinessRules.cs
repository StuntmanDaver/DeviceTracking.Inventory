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
/// Business rules for inventory operations
/// </summary>
public class InventoryBusinessRules
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly ISupplierRepository _supplierRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public InventoryBusinessRules(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository,
        IInventoryTransactionRepository transactionRepository,
        ISupplierRepository supplierRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
    }

    /// <summary>
    /// Validate stock level rules
    /// </summary>
    public async Task<ServiceResult> ValidateStockLevelAsync(Guid itemId, int newStockLevel, CancellationToken cancellationToken = default)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Prevent negative stock levels
        if (newStockLevel < 0)
        {
            return ServiceResult.Failure(ErrorMessages.InvalidStockLevel);
        }

        // Warn about overstocking
        if (newStockLevel > item.MaximumStock)
        {
            return ServiceResult.Failure($"Stock level ({newStockLevel}) exceeds maximum allowed ({item.MaximumStock})");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate barcode uniqueness
    /// </summary>
    public async Task<ServiceResult> ValidateBarcodeUniquenessAsync(string barcode, Guid? excludeItemId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return ServiceResult.Failure(ErrorMessages.InvalidBarcode);
        }

        var existingItem = await _inventoryItemRepository.GetByBarcodeAsync(barcode, cancellationToken);
        if (existingItem != null && existingItem.Id != excludeItemId)
        {
            return ServiceResult.Failure(ErrorMessages.DuplicateBarcode);
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate part number uniqueness
    /// </summary>
    public async Task<ServiceResult> ValidatePartNumberUniquenessAsync(string partNumber, Guid? excludeItemId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return ServiceResult.Failure("Part number cannot be empty");
        }

        var existingItem = await _inventoryItemRepository.GetByPartNumberAsync(partNumber, cancellationToken);
        if (existingItem != null && existingItem.Id != excludeItemId)
        {
            return ServiceResult.Failure(ErrorMessages.DuplicatePartNumber);
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate location hierarchy rules
    /// </summary>
    public async Task<ServiceResult> ValidateLocationHierarchyAsync(Guid? parentLocationId, Guid locationId, CancellationToken cancellationToken = default)
    {
        if (!parentLocationId.HasValue)
        {
            return ServiceResult.Success(); // Root level is always valid
        }

        // Prevent circular references
        var currentParentId = parentLocationId.Value;
        var visitedLocations = new HashSet<Guid> { locationId };

        while (currentParentId != Guid.Empty)
        {
            if (visitedLocations.Contains(currentParentId))
            {
                return ServiceResult.Failure("Circular reference detected in location hierarchy");
            }

            visitedLocations.Add(currentParentId);

            var parentLocation = await _locationRepository.GetByIdAsync(currentParentId, cancellationToken);
            if (parentLocation == null)
            {
                return ServiceResult.Failure("Parent location not found");
            }

            currentParentId = parentLocation.ParentLocationId ?? Guid.Empty;
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate transaction business rules
    /// </summary>
    public async Task<ServiceResult> ValidateTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
    {
        // Validate inventory item exists and is active
        var item = await _inventoryItemRepository.GetByIdAsync(transaction.InventoryItemId, cancellationToken);
        if (item == null || !item.IsActive)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Validate locations based on transaction type
        if (transaction.TransactionType == TransactionType.Receipt ||
            transaction.TransactionType == TransactionType.Adjustment ||
            transaction.TransactionType == TransactionType.CountAdjustment)
        {
            if (!transaction.DestinationLocationId.HasValue)
            {
                return ServiceResult.Failure("Destination location is required for this transaction type");
            }

            var destinationLocation = await _locationRepository.GetByIdAsync(transaction.DestinationLocationId.Value, cancellationToken);
            if (destinationLocation == null || !destinationLocation.IsActive)
            {
                return ServiceResult.Failure("Invalid destination location");
            }
        }

        if (transaction.TransactionType == TransactionType.Issue ||
            transaction.TransactionType == TransactionType.Adjustment)
        {
            if (!transaction.SourceLocationId.HasValue)
            {
                return ServiceResult.Failure("Source location is required for this transaction type");
            }

            var sourceLocation = await _locationRepository.GetByIdAsync(transaction.SourceLocationId.Value, cancellationToken);
            if (sourceLocation == null || !sourceLocation.IsActive)
            {
                return ServiceResult.Failure("Invalid source location");
            }

            // Check sufficient stock for issues
            if (transaction.TransactionType == TransactionType.Issue)
            {
                var availableStock = await GetAvailableStockAsync(transaction.InventoryItemId, transaction.SourceLocationId.Value, cancellationToken);
                if (availableStock < transaction.Quantity)
                {
                    return ServiceResult.Failure(ErrorMessages.InsufficientStock);
                }
            }
        }

        if (transaction.TransactionType == TransactionType.Transfer)
        {
            if (!transaction.SourceLocationId.HasValue || !transaction.DestinationLocationId.HasValue)
            {
                return ServiceResult.Failure("Both source and destination locations are required for transfers");
            }

            if (transaction.SourceLocationId == transaction.DestinationLocationId)
            {
                return ServiceResult.Failure("Source and destination locations cannot be the same");
            }

            var sourceStock = await GetAvailableStockAsync(transaction.InventoryItemId, transaction.SourceLocationId.Value, cancellationToken);
            if (sourceStock < transaction.Quantity)
            {
                return ServiceResult.Failure(ErrorMessages.InsufficientStock);
            }
        }

        // Validate quantity
        if (transaction.Quantity <= 0)
        {
            return ServiceResult.Failure("Transaction quantity must be greater than zero");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate supplier business rules
    /// </summary>
    public async Task<ServiceResult> ValidateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        // Validate supplier code uniqueness
        if (!string.IsNullOrWhiteSpace(supplier.Code))
        {
            var existingSupplier = await _supplierRepository.GetByCodeAsync(supplier.Code, cancellationToken);
            if (existingSupplier != null)
            {
                return ServiceResult.Failure(ErrorMessages.DuplicateSupplierCode);
            }
        }

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(supplier.Email) &&
            !IsValidEmail(supplier.Email))
        {
            return ServiceResult.Failure("Invalid email format");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Get available stock for an item at a specific location
    /// </summary>
    private async Task<int> GetAvailableStockAsync(Guid itemId, Guid locationId, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null || item.LocationId != locationId)
        {
            return 0;
        }

        return item.CurrentStock - item.ReservedStock;
    }

    /// <summary>
    /// Simple email validation
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate item can be deleted
    /// </summary>
    public async Task<ServiceResult> ValidateItemDeletionAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        // Check for active transactions
        var transactions = await _transactionRepository.GetByInventoryItemAsync(itemId, cancellationToken);
        var hasActiveTransactions = transactions.Any(t =>
            t.Status == TransactionStatus.Pending ||
            t.Status == TransactionStatus.Approved ||
            t.Status == TransactionStatus.Processing);

        if (hasActiveTransactions)
        {
            return ServiceResult.Failure(ErrorMessages.CannotDeleteActiveItem);
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate location can be deleted
    /// </summary>
    public async Task<ServiceResult> ValidateLocationDeletionAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        // Check for inventory items at this location
        var items = await _inventoryItemRepository.GetByLocationAsync(locationId, cancellationToken);
        if (items.Any())
        {
            return ServiceResult.Failure(ErrorMessages.CannotDeleteLocationWithItems);
        }

        // Check for transactions involving this location
        var transactions = await _transactionRepository.GetByLocationAsync(locationId, cancellationToken);
        if (transactions.Any())
        {
            return ServiceResult.Failure("Cannot delete location with transaction history");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Calculate reorder point based on usage patterns
    /// </summary>
    public async Task<ServiceResult<int>> CalculateReorderPointAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        // Get historical usage data (last 90 days)
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var transactions = await _transactionRepository.GetByInventoryItemAsync(itemId, cancellationToken);

        var issueTransactions = transactions.Where(t =>
            t.TransactionType == TransactionType.Issue &&
            t.ProcessedAt >= ninetyDaysAgo &&
            t.Status == TransactionStatus.Completed);

        if (!issueTransactions.Any())
        {
            return ServiceResult<int>.Failure("Insufficient transaction history to calculate reorder point");
        }

        // Calculate average daily usage
        var totalDays = 90;
        var totalIssued = issueTransactions.Sum(t => t.Quantity);
        var averageDailyUsage = (double)totalIssued / totalDays;

        // Get current item details
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult<int>.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Calculate reorder point: average daily usage * lead time days * safety factor
        var leadTimeDays = item.Supplier?.LeadTimeDays ?? 7;
        var safetyFactor = 1.2; // 20% safety stock

        var reorderPoint = (int)Math.Ceiling(averageDailyUsage * leadTimeDays * safetyFactor);

        return ServiceResult<int>.Success(reorderPoint);
    }
}
