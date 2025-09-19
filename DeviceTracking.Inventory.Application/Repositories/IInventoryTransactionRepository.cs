using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Repositories;

/// <summary>
/// Repository interface for inventory transaction operations with audit capabilities
/// </summary>
public interface IInventoryTransactionRepository
{
    /// <summary>
    /// Gets a transaction by its unique identifier
    /// </summary>
    Task<InventoryTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a transaction by its transaction number
    /// </summary>
    Task<InventoryTransaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by inventory item
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByInventoryItemAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by location (source or destination)
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by type
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by status
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByStatusAsync(TransactionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by user
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending transactions for approval
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a transaction number exists
    /// </summary>
    Task<bool> ExistsByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new transaction
    /// </summary>
    Task AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing transaction
    /// </summary>
    Task UpdateAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a transaction by its identifier
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending transaction
    /// </summary>
    Task ApproveTransactionAsync(Guid transactionId, string approvedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a transaction (moves it from pending to completed)
    /// </summary>
    Task ProcessTransactionAsync(Guid transactionId, string processedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a transaction
    /// </summary>
    Task CancelTransactionAsync(Guid transactionId, string cancelledBy, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transaction summary statistics
    /// </summary>
    Task<(int TotalTransactions, int PendingTransactions, int CompletedTransactions, decimal TotalValue)> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions for QuickBooks synchronization
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> GetTransactionsForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates QuickBooks sync status for a transaction
    /// </summary>
    Task UpdateQuickBooksSyncStatusAsync(Guid transactionId, bool isSynced, string? refId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the audit trail for a transaction
    /// </summary>
    Task<IEnumerable<(DateTime Timestamp, string Action, string User, string Details)>> GetTransactionAuditTrailAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
