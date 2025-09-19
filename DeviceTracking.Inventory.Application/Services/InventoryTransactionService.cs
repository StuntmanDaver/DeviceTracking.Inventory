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
/// Implementation of inventory transaction service
/// </summary>
public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryTransactionService(
        IInventoryTransactionRepository transactionRepository,
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<InventoryTransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Transaction not found");
            }

            var dto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error retrieving transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<InventoryTransactionDto>>> GetPagedAsync(InventoryTransactionQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply filters
            var filter = BuildFilter(query);
            var transactions = await _transactionRepository.FindAsync(filter, cancellationToken);
            var totalCount = await _transactionRepository.CountAsync(filter, cancellationToken);

            // Apply sorting
            var sortedTransactions = ApplySorting(transactions, query.SortBy, query.SortDirection);

            // Apply pagination
            var pagedTransactions = sortedTransactions
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var dtos = new List<InventoryTransactionDto>();
            foreach (var transaction in pagedTransactions)
            {
                dtos.Add(await MapTransactionToDtoAsync(transaction, cancellationToken));
            }

            var response = new PagedResponse<InventoryTransactionDto>
            {
                Items = dtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PagedResponse<InventoryTransactionDto>>.Ok(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<InventoryTransactionDto>>.Fail($"Error retrieving transactions: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> RecordReceiptAsync(CreateReceiptDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            var validationResult = await ValidateReceiptAsync(dto, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return ApiResponse<InventoryTransactionDto>.Fail(validationResult.Error);
            }

            // Create transaction
            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = await GenerateTransactionNumberAsync(TransactionType.Receipt, cancellationToken),
                Type = TransactionType.Receipt,
                TransactionDate = DateTime.UtcNow,
                InventoryItemId = dto.InventoryItemId,
                DestinationLocationId = dto.LocationId,
                Quantity = dto.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                ReferenceType = dto.ReferenceType,
                Notes = dto.Notes,
                PerformedBy = userId ?? "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            // Update inventory
            await ProcessReceiptAsync(dto, cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Receipt transaction recorded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error recording receipt transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> RecordIssueAsync(CreateIssueDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            var validationResult = await ValidateIssueAsync(dto, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return ApiResponse<InventoryTransactionDto>.Fail(validationResult.Error);
            }

            // Create transaction
            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = await GenerateTransactionNumberAsync(TransactionType.Issue, cancellationToken),
                Type = TransactionType.Issue,
                TransactionDate = DateTime.UtcNow,
                InventoryItemId = dto.InventoryItemId,
                SourceLocationId = dto.LocationId,
                Quantity = dto.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                ReferenceType = dto.ReferenceType,
                Notes = dto.Notes,
                PerformedBy = userId ?? "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            // Update inventory
            await ProcessIssueAsync(dto, cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Issue transaction recorded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error recording issue transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> RecordTransferAsync(CreateTransferDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            var validationResult = await ValidateTransferAsync(dto, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return ApiResponse<InventoryTransactionDto>.Fail(validationResult.Error);
            }

            // Create transaction
            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = await GenerateTransactionNumberAsync(TransactionType.Transfer, cancellationToken),
                Type = TransactionType.Transfer,
                TransactionDate = DateTime.UtcNow,
                InventoryItemId = dto.InventoryItemId,
                SourceLocationId = dto.SourceLocationId,
                DestinationLocationId = dto.DestinationLocationId,
                Quantity = dto.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                ReferenceType = dto.ReferenceType,
                Notes = dto.Notes,
                PerformedBy = userId ?? "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            // Update inventory
            await ProcessTransferAsync(dto, cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Transfer transaction recorded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error recording transfer transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> RecordAdjustmentAsync(CreateAdjustmentDto dto, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            var validationResult = await ValidateAdjustmentAsync(dto, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return ApiResponse<InventoryTransactionDto>.Fail(validationResult.Error);
            }

            // Create transaction
            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = await GenerateTransactionNumberAsync(TransactionType.Adjustment, cancellationToken),
                Type = TransactionType.Adjustment,
                TransactionDate = DateTime.UtcNow,
                InventoryItemId = dto.InventoryItemId,
                DestinationLocationId = dto.LocationId,
                Quantity = dto.QuantityAdjustment,
                ReferenceNumber = dto.AdjustmentReason,
                ReferenceType = "Adjustment",
                Notes = dto.Notes,
                PerformedBy = userId ?? "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            // Update inventory
            await ProcessAdjustmentAsync(dto, cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Adjustment transaction recorded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error recording adjustment transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> ApproveTransactionAsync(Guid transactionId, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Transaction not found");
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Only pending transactions can be approved");
            }

            transaction.Status = TransactionStatus.Approved;
            transaction.ApprovedBy = userId;
            transaction.ApprovedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.UpdatedBy = userId;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Transaction approved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error approving transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> ProcessTransactionAsync(Guid transactionId, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Transaction not found");
            }

            if (transaction.Status != TransactionStatus.Approved)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Only approved transactions can be processed");
            }

            // Process the transaction based on its type
            await ProcessApprovedTransactionAsync(transaction, cancellationToken);

            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedBy = userId;
            transaction.ProcessedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.UpdatedBy = userId;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Transaction processed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error processing transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<InventoryTransactionDto>> CancelTransactionAsync(Guid transactionId, string reason, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Transaction not found");
            }

            if (transaction.Status == TransactionStatus.Completed || transaction.Status == TransactionStatus.Cancelled)
            {
                return ApiResponse<InventoryTransactionDto>.Fail("Transaction cannot be cancelled");
            }

            transaction.Status = TransactionStatus.Cancelled;
            transaction.Notes = $"{transaction.Notes}\n\nCancelled: {reason}".Trim();
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.UpdatedBy = userId;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = await MapTransactionToDtoAsync(transaction, cancellationToken);
            return ApiResponse<InventoryTransactionDto>.Ok(resultDto, "Transaction cancelled successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<InventoryTransactionDto>.Fail($"Error cancelling transaction: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TransactionSummaryDto>> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionRepository.FindAsync(
                t => t.TransactionDate >= startDate && t.TransactionDate <= endDate,
                cancellationToken);

            var summary = new TransactionSummaryDto
            {
                TotalTransactions = transactions.Count(),
                PendingTransactions = transactions.Count(t => t.Status == TransactionStatus.Pending),
                ApprovedTransactions = transactions.Count(t => t.Status == TransactionStatus.Approved),
                CompletedTransactions = transactions.Count(t => t.Status == TransactionStatus.Completed),
                CancelledTransactions = transactions.Count(t => t.Status == TransactionStatus.Cancelled),
                TotalValue = transactions.Sum(t => t.UnitCost * t.Quantity ?? 0),
                StartDate = startDate,
                EndDate = endDate
            };

            if (summary.TotalTransactions > 0)
            {
                summary.AverageValue = summary.TotalValue / summary.TotalTransactions;
            }

            return ApiResponse<TransactionSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionSummaryDto>.Fail($"Error generating transaction summary: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryTransactionDto>>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionRepository.FindAsync(
                t => t.Status == TransactionStatus.Pending,
                cancellationToken);

            var dtos = new List<InventoryTransactionDto>();
            foreach (var transaction in transactions)
            {
                dtos.Add(await MapTransactionToDtoAsync(transaction, cancellationToken));
            }

            return ApiResponse<IEnumerable<InventoryTransactionDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<InventoryTransactionDto>>.Fail($"Error retrieving pending transactions: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryTransactionDto>>> GetForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionRepository.FindAsync(
                t => t.UpdatedAt >= sinceDate && t.Status == TransactionStatus.Completed,
                cancellationToken);

            var dtos = new List<InventoryTransactionDto>();
            foreach (var transaction in transactions)
            {
                dtos.Add(await MapTransactionToDtoAsync(transaction, cancellationToken));
            }

            return ApiResponse<IEnumerable<InventoryTransactionDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<InventoryTransactionDto>>.Fail($"Error retrieving transactions for QuickBooks sync: {ex.Message}");
        }
    }

    public async Task<ApiResponse> BulkProcessAsync(IEnumerable<Guid> transactionIds, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var transactionId in transactionIds)
            {
                var result = await ProcessTransactionAsync(transactionId, userId, cancellationToken);
                if (!result.IsSuccess)
                {
                    return ApiResponse.Fail($"Failed to process transaction {transactionId}: {result.Error}");
                }
            }

            return ApiResponse.Ok($"{transactionIds.Count()} transactions processed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error processing bulk transactions: {ex.Message}");
        }
    }

    // Private helper methods

    private async Task<InventoryTransactionDto> MapTransactionToDtoAsync(InventoryTransaction transaction, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<InventoryTransactionDto>(transaction);

        // Calculate total cost
        dto.TotalCost = transaction.UnitCost * transaction.Quantity;

        return dto;
    }

    private async Task<string> GenerateTransactionNumberAsync(TransactionType type, CancellationToken cancellationToken)
    {
        return await _transactionRepository.GenerateTransactionNumberAsync(cancellationToken) ?? $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private async Task<ServiceResult> ValidateReceiptAsync(CreateReceiptDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
            return ServiceResult.Failure("Receipt quantity must be greater than zero");

        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
            return ServiceResult.Failure("Inventory item not found");

        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
            return ServiceResult.Failure("Location not found");

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateIssueAsync(CreateIssueDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
            return ServiceResult.Failure("Issue quantity must be greater than zero");

        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
            return ServiceResult.Failure("Inventory item not found");

        if (item.CurrentStock < dto.Quantity)
            return ServiceResult.Failure("Insufficient stock for issue");

        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
            return ServiceResult.Failure("Location not found");

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateTransferAsync(CreateTransferDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
            return ServiceResult.Failure("Transfer quantity must be greater than zero");

        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
            return ServiceResult.Failure("Inventory item not found");

        if (item.CurrentStock < dto.Quantity)
            return ServiceResult.Failure("Insufficient stock for transfer");

        var sourceLocation = await _locationRepository.GetByIdAsync(dto.SourceLocationId, cancellationToken);
        if (sourceLocation == null)
            return ServiceResult.Failure("Source location not found");

        var destLocation = await _locationRepository.GetByIdAsync(dto.DestinationLocationId, cancellationToken);
        if (destLocation == null)
            return ServiceResult.Failure("Destination location not found");

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateAdjustmentAsync(CreateAdjustmentDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null)
            return ServiceResult.Failure("Inventory item not found");

        if (dto.QuantityAdjustment == 0)
            return ServiceResult.Failure("Adjustment quantity cannot be zero");

        // Check if adjustment would result in negative stock
        if (dto.QuantityAdjustment < 0 && item.CurrentStock < Math.Abs(dto.QuantityAdjustment))
            return ServiceResult.Failure("Adjustment would result in negative stock");

        var location = await _locationRepository.GetByIdAsync(dto.LocationId, cancellationToken);
        if (location == null)
            return ServiceResult.Failure("Location not found");

        return ServiceResult.Success();
    }

    private async Task ProcessReceiptAsync(CreateReceiptDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item != null)
        {
            item.CurrentStock += dto.Quantity;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            if (dto.UnitCost.HasValue)
            {
                item.StandardCost = dto.UnitCost.Value;
            }

            _inventoryItemRepository.Update(item);
        }
    }

    private async Task ProcessIssueAsync(CreateIssueDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item != null)
        {
            item.CurrentStock -= dto.Quantity;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _inventoryItemRepository.Update(item);
        }
    }

    private async Task ProcessTransferAsync(CreateTransferDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item != null)
        {
            item.CurrentStock -= dto.Quantity;
            item.LocationId = dto.DestinationLocationId;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _inventoryItemRepository.Update(item);
        }
    }

    private async Task ProcessAdjustmentAsync(CreateAdjustmentDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item != null)
        {
            item.CurrentStock += dto.QuantityAdjustment;
            item.LastMovement = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _inventoryItemRepository.Update(item);
        }
    }

    private async Task ProcessApprovedTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken)
    {
        // This would process the actual inventory changes for approved transactions
        // Implementation depends on the specific business requirements
    }

    private System.Linq.Expressions.Expression<Func<InventoryTransaction, bool>> BuildFilter(InventoryTransactionQueryDto query)
    {
        return transaction =>
            (!query.StartDate.HasValue || transaction.TransactionDate >= query.StartDate.Value) &&
            (!query.EndDate.HasValue || transaction.TransactionDate <= query.EndDate.Value) &&
            (string.IsNullOrEmpty(query.TransactionType) || transaction.Type.ToString() == query.TransactionType) &&
            (string.IsNullOrEmpty(query.Status) || transaction.Status.ToString() == query.Status) &&
            (!query.InventoryItemId.HasValue || transaction.InventoryItemId == query.InventoryItemId.Value) &&
            (!query.LocationId.HasValue || transaction.SourceLocationId == query.LocationId.Value || transaction.DestinationLocationId == query.LocationId.Value) &&
            (string.IsNullOrEmpty(query.InitiatedBy) || transaction.PerformedBy.Contains(query.InitiatedBy)) &&
            (string.IsNullOrEmpty(query.ReferenceNumber) || transaction.ReferenceNumber.Contains(query.ReferenceNumber)) &&
            (!query.MinQuantity.HasValue || transaction.Quantity >= query.MinQuantity.Value) &&
            (!query.MaxQuantity.HasValue || transaction.Quantity <= query.MaxQuantity.Value) &&
            (string.IsNullOrEmpty(query.SearchTerm) ||
             transaction.TransactionNumber.Contains(query.SearchTerm) ||
             transaction.Notes.Contains(query.SearchTerm));
    }

    private IEnumerable<InventoryTransaction> ApplySorting(IEnumerable<InventoryTransaction> transactions, string? sortBy, SortDirection sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.TransactionDate)
                : transactions.OrderBy(t => t.TransactionDate);
        }

        return sortBy.ToLower() switch
        {
            "number" => sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.TransactionNumber)
                : transactions.OrderBy(t => t.TransactionNumber),
            "date" => sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.TransactionDate)
                : transactions.OrderBy(t => t.TransactionDate),
            "type" => sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.Type)
                : transactions.OrderBy(t => t.Type),
            "quantity" => sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.Quantity)
                : transactions.OrderBy(t => t.Quantity),
            _ => sortDirection == SortDirection.Descending
                ? transactions.OrderByDescending(t => t.TransactionDate)
                : transactions.OrderBy(t => t.TransactionDate)
        };
    }
}
