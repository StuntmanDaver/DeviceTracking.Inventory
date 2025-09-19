using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using DeviceTracking.Inventory.Shared.Entities;
using System.Reflection;

namespace DeviceTracking.Inventory.Infrastructure.Data;

/// <summary>
/// Database context for the Inventory system
/// </summary>
public class InventoryDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Constructor for design-time operations (migrations)
    /// </summary>
    public InventoryDbContext()
    {
    }

    /// <summary>
    /// Constructor with configuration for design-time operations
    /// </summary>
    public InventoryDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Inventory items table
    /// </summary>
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;

    /// <summary>
    /// Locations table
    /// </summary>
    public DbSet<Location> Locations { get; set; } = null!;

    /// <summary>
    /// Inventory transactions table
    /// </summary>
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;

    /// <summary>
    /// Suppliers table
    /// </summary>
    public DbSet<Supplier> Suppliers { get; set; } = null!;

    /// <summary>
    /// Configures the database context
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // For design-time operations, use localdb
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=DeviceTracking_Inventory;Trusted_Connection=True;MultipleActiveResultSets=true",
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: new[] { 4060, 10928, 10929, 40197, 40501, 40613 });
                    sqlOptions.CommandTimeout(30);
                });
        }
    }

    /// <summary>
    /// Configures the database model
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure default schema
        modelBuilder.HasDefaultSchema("Inventory");

        // Configure entity relationships and constraints
        ConfigureRelationships(modelBuilder);

        // Configure indexes for performance
        ConfigureIndexes(modelBuilder);

        // Configure default values and computed columns
        ConfigureDefaults(modelBuilder);
    }

    /// <summary>
    /// Configures entity relationships
    /// </summary>
    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // InventoryItem -> Location (many-to-one)
        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Location)
            .WithMany(l => l.InventoryItems)
            .HasForeignKey(i => i.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryItem -> Supplier (many-to-one)
        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Supplier)
            .WithMany(s => s.InventoryItems)
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Location -> Parent Location (self-referencing)
        modelBuilder.Entity<Location>()
            .HasOne(l => l.ParentLocation)
            .WithMany(l => l.ChildLocations)
            .HasForeignKey(l => l.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryTransaction -> InventoryItem (many-to-one)
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(t => t.InventoryItem)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryTransaction -> Source Location (many-to-one)
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(t => t.SourceLocation)
            .WithMany(l => l.SourceTransactions)
            .HasForeignKey(t => t.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryTransaction -> Destination Location (many-to-one)
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(t => t.DestinationLocation)
            .WithMany(l => l.DestinationTransactions)
            .HasForeignKey(t => t.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// Configures database indexes for performance
    /// </summary>
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Composite index for common queries: Location + Active + LastMovement
        modelBuilder.Entity<InventoryItem>()
            .HasIndex(i => new { i.LocationId, i.IsActive, i.LastMovement })
            .HasDatabaseName("IX_InventoryItems_Location_Active_LastMovement");

        // Index for transaction queries by date
        modelBuilder.Entity<InventoryTransaction>()
            .HasIndex(t => new { t.TransactionType, t.Status, t.ProcessedAt })
            .HasDatabaseName("IX_InventoryTransactions_Type_Status_ProcessedAt");

        // Index for location hierarchy queries
        modelBuilder.Entity<Location>()
            .HasIndex(l => new { l.ParentLocationId, l.IsActive })
            .HasDatabaseName("IX_Locations_Parent_Active");
    }

    /// <summary>
    /// Configures default values and computed columns
    /// </summary>
    private static void ConfigureDefaults(ModelBuilder modelBuilder)
    {
        // Set default values for audit fields
        modelBuilder.Entity<InventoryItem>()
            .Property(i => i.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Location>()
            .Property(l => l.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<InventoryTransaction>()
            .Property(t => t.InitiatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Supplier>()
            .Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure enum conversions
        modelBuilder.Entity<Location>()
            .Property(l => l.LocationType)
            .HasConversion<string>();

        modelBuilder.Entity<InventoryTransaction>()
            .Property(t => t.TransactionType)
            .HasConversion<string>();

        modelBuilder.Entity<InventoryTransaction>()
            .Property(t => t.Status)
            .HasConversion<string>();
    }

    /// <summary>
    /// Saves changes with audit trail updates
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves changes with audit trail updates
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Updates audit fields for changed entities
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (IAuditable)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

/// <summary>
/// Interface for auditable entities
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
