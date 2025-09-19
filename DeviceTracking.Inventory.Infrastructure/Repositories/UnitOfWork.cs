using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeviceTracking.Inventory.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions across repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly InventoryDbContext _context;
    private IDbContextTransaction? _transaction;

    private IInventoryItemRepository? _inventoryItems;
    private ILocationRepository? _locations;
    private IInventoryTransactionRepository? _transactions;
    private ISupplierRepository? _suppliers;

    /// <summary>
    /// Constructor
    /// </summary>
    public UnitOfWork(InventoryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the inventory item repository
    /// </summary>
    public IInventoryItemRepository InventoryItems =>
        _inventoryItems ??= new InventoryItemRepository(_context);

    /// <summary>
    /// Gets the location repository
    /// </summary>
    public ILocationRepository Locations =>
        _locations ??= new LocationRepository(_context);

    /// <summary>
    /// Gets the transaction repository
    /// </summary>
    public IInventoryTransactionRepository Transactions =>
        _transactions ??= new InventoryTransactionRepository(_context);

    /// <summary>
    /// Gets the supplier repository
    /// </summary>
    public ISupplierRepository Suppliers =>
        _suppliers ??= new SupplierRepository(_context);

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the context and transaction
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the context and transaction asynchronously
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
