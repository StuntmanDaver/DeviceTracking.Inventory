using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Infrastructure.Data;
using DeviceTracking.Inventory.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceTracking.Inventory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for inventory transaction operations
/// </summary>
public class InventoryTransactionRepository : Repository<InventoryTransaction>, IInventoryTransactionRepository
{
    /// <summary>
    /// Constructor
    /// </summary>
    public InventoryTransactionRepository(InventoryDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a transaction by its transaction number
    /// </summary>
    public async Task<InventoryTransaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber, cancellationToken);
    }

    /// <summary>
    /// Gets transactions by inventory item
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByInventoryItemAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.InventoryItemId == inventoryItemId)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets transactions by location (source or destination)
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.SourceLocationId == locationId || t.DestinationLocationId == locationId)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets transactions by type
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets transactions by status
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByStatusAsync(TransactionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.InitiatedAt >= startDate && t.InitiatedAt <= endDate)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets transactions by user
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.InitiatedBy == userId)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets pending transactions for approval
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => t.Status == TransactionStatus.Pending)
            .OrderBy(t => t.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a transaction number exists
    /// </summary>
    public async Task<bool> ExistsByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .AnyAsync(t => t.TransactionNumber == transactionNumber, cancellationToken);
    }

    /// <summary>
    /// Approves a pending transaction
    /// </summary>
    public async Task ApproveTransactionAsync(Guid transactionId, string approvedBy, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(transactionId, cancellationToken);
        if (transaction != null && transaction.Status == TransactionStatus.Pending)
        {
            transaction.Status = TransactionStatus.Approved;
            transaction.ApprovedBy = approvedBy;
            transaction.ApprovedAt = DateTime.UtcNow;
            await UpdateAsync(transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a transaction (moves it from approved to completed)
    /// </summary>
    public async Task ProcessTransactionAsync(Guid transactionId, string processedBy, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(transactionId, cancellationToken);
        if (transaction != null && transaction.Status == TransactionStatus.Approved)
        {
            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedBy = processedBy;
            transaction.ProcessedAt = DateTime.UtcNow;
            await UpdateAsync(transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Cancels a transaction
    /// </summary>
    public async Task CancelTransactionAsync(Guid transactionId, string cancelledBy, string reason, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(transactionId, cancellationToken);
        if (transaction != null && transaction.Status != TransactionStatus.Completed)
        {
            transaction.Status = TransactionStatus.Cancelled;
            transaction.Notes = $"{transaction.Notes} | Cancelled by {cancelledBy}: {reason}".Trim();
            await UpdateAsync(transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Gets transaction summary statistics
    /// </summary>
    public async Task<(int TotalTransactions, int PendingTransactions, int CompletedTransactions, decimal TotalValue)> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.InventoryTransactions
            .Where(t => t.InitiatedAt >= startDate && t.InitiatedAt <= endDate)
            .ToListAsync(cancellationToken);

        return (
            transactions.Count,
            transactions.Count(t => t.Status == TransactionStatus.Pending),
            transactions.Count(t => t.Status == TransactionStatus.Completed),
            transactions.Where(t => t.Status == TransactionStatus.Completed).Sum(t => t.TotalCost)
        );
    }

    /// <summary>
    /// Gets transactions for QuickBooks synchronization
    /// </summary>
    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsForQuickBooksSyncAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Where(t => !t.IsQuickBooksSynced && t.Status == TransactionStatus.Completed &&
                       t.ProcessedAt >= sinceDate)
            .OrderBy(t => t.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates QuickBooks sync status for a transaction
    /// </summary>
    public async Task UpdateQuickBooksSyncStatusAsync(Guid transactionId, bool isSynced, string? refId = null, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(transactionId, cancellationToken);
        if (transaction != null)
        {
            transaction.IsQuickBooksSynced = isSynced;
            transaction.QuickBooksRefId = refId;
            await UpdateAsync(transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the audit trail for a transaction
    /// </summary>
    public async Task<IEnumerable<(DateTime Timestamp, string Action, string User, string Details)>> GetTransactionAuditTrailAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(transactionId, cancellationToken);
        if (transaction == null)
        {
            return Enumerable.Empty<(DateTime, string, string, string)>();
        }

        var auditTrail = new List<(DateTime Timestamp, string Action, string User, string Details)>();

        auditTrail.Add((transaction.InitiatedAt, "Created", transaction.InitiatedBy, "Transaction initiated"));

        if (transaction.ApprovedAt.HasValue && transaction.ApprovedBy != null)
        {
            auditTrail.Add((transaction.ApprovedAt.Value, "Approved", transaction.ApprovedBy, "Transaction approved"));
        }

        if (transaction.ProcessedAt.HasValue && transaction.ProcessedBy != null)
        {
            auditTrail.Add((transaction.ProcessedAt.Value, "Processed", transaction.ProcessedBy, "Transaction completed"));
        }

        return auditTrail.OrderBy(a => a.Timestamp);
    }
}
