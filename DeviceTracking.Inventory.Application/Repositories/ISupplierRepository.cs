using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Repositories;

/// <summary>
/// Repository interface for supplier operations
/// </summary>
public interface ISupplierRepository
{
    /// <summary>
    /// Gets a supplier by its unique identifier
    /// </summary>
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a supplier by its code
    /// </summary>
    Task<Supplier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all suppliers
    /// </summary>
    Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active suppliers
    /// </summary>
    Task<IEnumerable<Supplier>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suppliers by rating
    /// </summary>
    Task<IEnumerable<Supplier>> GetByRatingAsync(int minRating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a supplier exists by code
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new supplier
    /// </summary>
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a supplier by its identifier
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets supplier performance metrics
    /// </summary>
    Task<IEnumerable<(Supplier Supplier, int TotalOrders, decimal AverageLeadTime, decimal OnTimeDeliveryRate)>> GetSupplierPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
