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
/// Repository implementation for supplier operations
/// </summary>
public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    /// <summary>
    /// Constructor
    /// </summary>
    public SupplierRepository(InventoryDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a supplier by its code
    /// </summary>
    public async Task<Supplier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .Include(s => s.InventoryItems)
            .FirstOrDefaultAsync(s => s.Code == code && s.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all active suppliers
    /// </summary>
    public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .Include(s => s.InventoryItems)
            .Where(s => s.IsActive)
            .OrderBy(s => s.CompanyName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets suppliers by rating
    /// </summary>
    public async Task<IEnumerable<Supplier>> GetByRatingAsync(int minRating, CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .Include(s => s.InventoryItems)
            .Where(s => s.IsActive && s.Rating >= minRating)
            .OrderByDescending(s => s.Rating)
            .ThenBy(s => s.CompanyName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a supplier exists by code
    /// </summary>
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .AnyAsync(s => s.Code == code, cancellationToken);
    }

    /// <summary>
    /// Gets supplier performance metrics
    /// </summary>
    public async Task<IEnumerable<(Supplier Supplier, int TotalOrders, decimal AverageLeadTime, decimal OnTimeDeliveryRate)>> GetSupplierPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // In a real scenario, this would analyze actual transaction data
        var suppliers = await _context.Suppliers
            .Include(s => s.InventoryItems)
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        return suppliers.Select(s =>
        {
            var totalItems = s.InventoryItems?.Count ?? 0;
            var leadTime = s.LeadTimeDays ?? 0;
            var rating = s.Rating ?? 3;

            // Simplified performance calculation
            var onTimeDeliveryRate = Math.Min(100, rating * 20); // Rating 5 = 100%, Rating 1 = 20%

            return (s, totalItems, (decimal)leadTime, (decimal)onTimeDeliveryRate);
        });
    }
}
