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
/// Business rules for inventory transactions
/// </summary>
public class TransactionBusinessRules
{
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public TransactionBusinessRules(
        IInventoryTransactionRepository transactionRepository,
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    }

    /// <summary>
    /// Validate receipt transaction
    /// </summary>
    public async Task<ServiceResult> ValidateReceiptAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (dto.Quantity <= 0)
        {
            return ServiceResult.Failure("Receipt quantity must be greater than zero");
        }

        // Validate item exists
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Validate destination location
        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
        {
            return ServiceResult.Failure(ErrorMessages.LocationNotFound);
        }

        // Validate location can receive items
        if (!CanLocationReceiveItems(location.Type))
        {
            return ServiceResult.Failure($"Location type '{location.Type}' cannot receive inventory items");
        }

        // Check for reasonable quantity limits
        if (dto.Quantity > 1000000) // 1 million units
        {
            return ServiceResult.Failure("Receipt quantity exceeds reasonable limits");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate issue transaction
    /// </summary>
    public async Task<ServiceResult> ValidateIssueAsync(CreateIssueDto dto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (dto.Quantity <= 0)
        {
            return ServiceResult.Failure("Issue quantity must be greater than zero");
        }

        // Validate item exists
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Validate source location
        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
        {
            return ServiceResult.Failure(ErrorMessages.LocationNotFound);
        }

        // Check sufficient stock
        var availableStock = await GetAvailableStockAsync(dto.InventoryItemId, dto.LocationId, cancellationToken);
        if (availableStock < dto.Quantity)
        {
            return ServiceResult.Failure($"Insufficient stock. Available: {availableStock}, Requested: {dto.Quantity}");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate transfer transaction
    /// </summary>
    public async Task<ServiceResult> ValidateTransferAsync(CreateTransferDto dto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (dto.Quantity <= 0)
        {
            return ServiceResult.Failure("Transfer quantity must be greater than zero");
        }

        if (dto.SourceLocationId == dto.DestinationLocationId)
        {
            return ServiceResult.Failure("Source and destination locations cannot be the same");
        }

        // Validate item exists
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Validate source location
        var sourceLocation = await _locationRepository.GetByIdAsync(dto.SourceLocationId, cancellationToken);
        if (sourceLocation == null)
        {
            return ServiceResult.Failure("Source location not found");
        }

        // Validate destination location
        var destinationLocation = await _locationRepository.GetByIdAsync(dto.DestinationLocationId, cancellationToken);
        if (destinationLocation == null)
        {
            return ServiceResult.Failure("Destination location not found");
        }

        // Check sufficient stock at source
        var availableStock = await GetAvailableStockAsync(dto.InventoryItemId, dto.SourceLocationId, cancellationToken);
        if (availableStock < dto.Quantity)
        {
            return ServiceResult.Failure($"Insufficient stock at source location. Available: {availableStock}, Requested: {dto.Quantity}");
        }

        // Validate destination can receive items
        if (!CanLocationReceiveItems(destinationLocation.Type))
        {
            return ServiceResult.Failure($"Destination location type '{destinationLocation.Type}' cannot receive inventory items");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate adjustment transaction
    /// </summary>
    public async Task<ServiceResult> ValidateAdjustmentAsync(CreateAdjustmentDto dto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.AdjustmentReason))
        {
            return ServiceResult.Failure("Adjustment reason is required");
        }

        // Validate item exists
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
        {
            return ServiceResult.Failure(ErrorMessages.InventoryItemNotFound);
        }

        // Validate location
        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
        {
            return ServiceResult.Failure(ErrorMessages.LocationNotFound);
        }

        // For negative adjustments, check sufficient stock
        if (dto.QuantityAdjustment < 0)
        {
            var availableStock = await GetAvailableStockAsync(dto.InventoryItemId, dto.LocationId, cancellationToken);
            if (availableStock < Math.Abs(dto.QuantityAdjustment))
            {
                return ServiceResult.Failure($"Cannot adjust below zero stock. Available: {availableStock}, Adjustment: {dto.QuantityAdjustment}");
            }
        }

        // Check for reasonable adjustment limits
        var absoluteAdjustment = Math.Abs(dto.QuantityAdjustment);
        if (absoluteAdjustment > item.CurrentStock * 2 && item.CurrentStock > 0)
        {
            return ServiceResult.Failure("Adjustment quantity seems unreasonably large compared to current stock");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Validate transaction state transition
    /// </summary>
    public ServiceResult ValidateStateTransition(TransactionStatus currentStatus, TransactionStatus newStatus)
    {
        // Define valid state transitions
        var validTransitions = new Dictionary<TransactionStatus, TransactionStatus[]>
        {
            [TransactionStatus.Pending] = new[] { TransactionStatus.Approved, TransactionStatus.Cancelled },
            [TransactionStatus.Approved] = new[] { TransactionStatus.Processing, TransactionStatus.Cancelled },
            [TransactionStatus.Processing] = new[] { TransactionStatus.Completed, TransactionStatus.Failed },
            [TransactionStatus.Completed] = Array.Empty<TransactionStatus>(),
            [TransactionStatus.Cancelled] = Array.Empty<TransactionStatus>(),
            [TransactionStatus.Failed] = new[] { TransactionStatus.Pending } // Can retry failed transactions
        };

        if (validTransitions.TryGetValue(currentStatus, out var allowedTransitions))
        {
            if (!allowedTransitions.Contains(newStatus))
            {
                return ServiceResult.Failure($"Invalid state transition from {currentStatus} to {newStatus}");
            }
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Check if transaction can be modified
    /// </summary>
    public ServiceResult ValidateTransactionModification(InventoryTransaction transaction)
    {
        var nonModifiableStatuses = new[] { TransactionStatus.Completed, TransactionStatus.Processing };

        if (nonModifiableStatuses.Contains(transaction.Status))
        {
            return ServiceResult.Failure($"Transaction in status '{transaction.Status}' cannot be modified");
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Calculate available stock for an item at a location
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
    /// Check if location type can receive items
    /// </summary>
    private bool CanLocationReceiveItems(LocationType locationType)
    {
        var receivableTypes = new[]
        {
            LocationType.Warehouse,
            LocationType.ProductionFloor,
            LocationType.CustomerSite,
            LocationType.Other
        };

        return receivableTypes.Contains(locationType);
    }

    /// <summary>
    /// Generate transaction number
    /// </summary>
    public async Task<ServiceResult<string>> GenerateTransactionNumberAsync(TransactionType type, CancellationToken cancellationToken = default)
    {
        var prefix = GetTransactionPrefix(type);
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

        // Get the last transaction number for today
        var lastTransactionNumber = await GetLastTransactionNumberAsync(prefix, datePart, cancellationToken);

        int sequenceNumber = 1;
        if (!string.IsNullOrEmpty(lastTransactionNumber))
        {
            // Extract sequence number from last transaction
            var parts = lastTransactionNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var lastSequence))
            {
                sequenceNumber = lastSequence + 1;
            }
        }

        var transactionNumber = $"{prefix}-{datePart}-{sequenceNumber:D4}";
        return ServiceResult<string>.Success(transactionNumber);
    }

    /// <summary>
    /// Get transaction prefix based on type
    /// </summary>
    private string GetTransactionPrefix(TransactionType type)
    {
        return type switch
        {
            TransactionType.Receipt => "REC",
            TransactionType.Issue => "ISS",
            TransactionType.Transfer => "TRF",
            TransactionType.Adjustment => "ADJ",
            TransactionType.CycleCount => "CC",
            TransactionType.Return => "RTN",
            _ => "TXN"
        };
    }

    /// <summary>
    /// Get the last transaction number for the given prefix and date
    /// </summary>
    private async Task<string?> GetLastTransactionNumberAsync(string prefix, string datePart, CancellationToken cancellationToken)
    {
        // This would typically query the database for the last transaction number
        // For now, return null (first transaction of the day)
        return null;
    }

    /// <summary>
    /// Validate bulk transaction processing
    /// </summary>
    public ServiceResult ValidateBulkTransactions(IEnumerable<InventoryTransaction> transactions)
    {
        var errors = new List<string>();

        // Check for duplicate items in the same batch
        var itemGroups = transactions.GroupBy(t => new { t.InventoryItemId, t.SourceLocationId, t.DestinationLocationId });
        foreach (var group in itemGroups)
        {
            if (group.Count() > 1)
            {
                errors.Add($"Multiple transactions for the same item and location combination");
                break;
            }
        }

        // Check total batch size
        if (transactions.Count() > 100)
        {
            errors.Add("Batch size cannot exceed 100 transactions");
        }

        // Validate each transaction
        foreach (var transaction in transactions)
        {
            if (transaction.Quantity <= 0)
            {
                errors.Add("All transactions must have positive quantity");
                break;
            }
        }

        if (errors.Any())
        {
            return ServiceResult.Failure(string.Join("; ", errors));
        }

        return ServiceResult.Success();
    }

    /// <summary>
    /// Calculate transaction impact on inventory
    /// </summary>
    public ServiceResult<int> CalculateInventoryImpact(InventoryTransaction transaction)
    {
        var impact = transaction.TransactionType switch
        {
            TransactionType.Receipt => transaction.Quantity,
            TransactionType.Issue => -transaction.Quantity,
            TransactionType.Transfer => transaction.SourceLocationId.HasValue ? -transaction.Quantity : transaction.Quantity,
            TransactionType.Adjustment => transaction.Quantity, // Quantity can be positive or negative
            TransactionType.CycleCount => transaction.Quantity - transaction.Item?.CurrentStock ?? 0,
            TransactionType.Return => transaction.Quantity,
            _ => 0
        };

        return ServiceResult<int>.Success(impact);
    }

    /// <summary>
    /// Validate transaction approval permissions
    /// </summary>
    public ServiceResult ValidateApprovalPermissions(InventoryTransaction transaction, string userRole)
    {
        // Define approval limits by role
        var roleLimits = new Dictionary<string, decimal>
        {
            ["Clerk"] = 1000,      // Can approve up to $1000
            ["Manager"] = 10000,   // Can approve up to $10,000
            ["Admin"] = 100000     // Can approve up to $100,000
        };

        if (!roleLimits.TryGetValue(userRole, out var limit))
        {
            return ServiceResult.Failure($"Unknown user role: {userRole}");
        }

        var transactionValue = (transaction.UnitCost ?? 0) * transaction.Quantity;
        if (transactionValue > limit)
        {
            return ServiceResult.Failure($"Transaction value (${transactionValue}) exceeds approval limit for {userRole} (${limit})");
        }

        return ServiceResult.Success();
    }
}
