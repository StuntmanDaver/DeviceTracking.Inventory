using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.DTOs;

namespace DeviceTracking.Inventory.Application.Services;

/// <summary>
/// Service interface for inventory transaction operations
/// </summary>
public interface IInventoryTransactionService
{
    /// <summary>
    /// Get a transaction by ID
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions with pagination and filtering
    /// </summary>
    Task<ApiResponse<PagedResponse<InventoryTransactionDto>>> GetPagedAsync(InventoryTransactionQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a receipt transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> RecordReceiptAsync(CreateReceiptDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record an issue transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> RecordIssueAsync(CreateIssueDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a transfer transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> RecordTransferAsync(CreateTransferDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record an adjustment transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> RecordAdjustmentAsync(CreateAdjustmentDto dto, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a pending transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> ApproveTransactionAsync(Guid transactionId, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> ProcessTransactionAsync(Guid transactionId, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a transaction
    /// </summary>
    Task<ApiResponse<InventoryTransactionDto>> CancelTransactionAsync(Guid transactionId, string reason, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction summary statistics
    /// </summary>
    Task<ApiResponse<TransactionSummaryDto>> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending transactions for approval
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryTransactionDto>>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions for QuickBooks synchronization
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryTransactionDto>>> GetForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk process transactions
    /// </summary>
    Task<ApiResponse> BulkProcessAsync(IEnumerable<Guid> transactionIds, string? userId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for creating a receipt transaction
/// </summary>
public class CreateReceiptDto
{
    /// <summary>
    /// Inventory item ID
    /// </summary>
    public Guid InventoryItemId { get; set; }

    /// <summary>
    /// Location ID where items are being received
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Quantity being received
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost of the items
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Reference number (PO number, etc.)
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Reference type
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Supplier ID
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Notes about the receipt
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating an issue transaction
/// </summary>
public class CreateIssueDto
{
    /// <summary>
    /// Inventory item ID
    /// </summary>
    public Guid InventoryItemId { get; set; }

    /// <summary>
    /// Location ID where items are being issued from
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Quantity being issued
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Reference number (work order, etc.)
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Reference type
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Notes about the issue
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a transfer transaction
/// </summary>
public class CreateTransferDto
{
    /// <summary>
    /// Inventory item ID
    /// </summary>
    public Guid InventoryItemId { get; set; }

    /// <summary>
    /// Source location ID
    /// </summary>
    public Guid SourceLocationId { get; set; }

    /// <summary>
    /// Destination location ID
    /// </summary>
    public Guid DestinationLocationId { get; set; }

    /// <summary>
    /// Quantity being transferred
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Reference number
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Reference type
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Notes about the transfer
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating an adjustment transaction
/// </summary>
public class CreateAdjustmentDto
{
    /// <summary>
    /// Inventory item ID
    /// </summary>
    public Guid InventoryItemId { get; set; }

    /// <summary>
    /// Location ID
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Quantity adjustment (positive for increase, negative for decrease)
    /// </summary>
    public int QuantityAdjustment { get; set; }

    /// <summary>
    /// Reason for the adjustment
    /// </summary>
    public string AdjustmentReason { get; set; } = string.Empty;

    /// <summary>
    /// Unit cost for the adjustment
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Notes about the adjustment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for transaction response
/// </summary>
public class InventoryTransactionDto
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Inventory item information
    /// </summary>
    public InventoryItemSummaryDto? InventoryItem { get; set; }

    /// <summary>
    /// Source location
    /// </summary>
    public LocationSummaryDto? SourceLocation { get; set; }

    /// <summary>
    /// Destination location
    /// </summary>
    public LocationSummaryDto? DestinationLocation { get; set; }

    /// <summary>
    /// Quantity involved
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Total cost
    /// </summary>
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// Reference number
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Reference type
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Adjustment reason (for adjustments)
    /// </summary>
    public string? AdjustmentReason { get; set; }

    /// <summary>
    /// User who initiated the transaction
    /// </summary>
    public string InitiatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When the transaction was initiated
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// User who approved the transaction
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// When the transaction was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// User who processed the transaction
    /// </summary>
    public string? ProcessedBy { get; set; }

    /// <summary>
    /// When the transaction was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Transaction notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether synced with QuickBooks
    /// </summary>
    public bool IsQuickBooksSynced { get; set; }
}

/// <summary>
/// DTO for transaction summary statistics
/// </summary>
public class TransactionSummaryDto
{
    /// <summary>
    /// Total number of transactions
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Number of pending transactions
    /// </summary>
    public int PendingTransactions { get; set; }

    /// <summary>
    /// Number of approved transactions
    /// </summary>
    public int ApprovedTransactions { get; set; }

    /// <summary>
    /// Number of completed transactions
    /// </summary>
    public int CompletedTransactions { get; set; }

    /// <summary>
    /// Number of cancelled transactions
    /// </summary>
    public int CancelledTransactions { get; set; }

    /// <summary>
    /// Total value of all transactions
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageValue { get; set; }

    /// <summary>
    /// Date range for the summary
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date range for the summary
    /// </summary>
    public DateTime EndDate { get; set; }
}
