using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Infrastructure.Data;
using DeviceTracking.Inventory.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceTracking.Inventory.IntegrationTests.TestData;

/// <summary>
/// Seeds test data for integration tests
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public static async Task SeedTestDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Clear existing data
        await ClearExistingDataAsync(context);

        // Seed test data
        await SeedLocationsAsync(context);
        await SeedSuppliersAsync(context);
        await SeedInventoryItemsAsync(context);
        await SeedInventoryTransactionsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task ClearExistingDataAsync(InventoryDbContext context)
    {
        // Clear in reverse order of dependencies
        context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
        context.InventoryItems.RemoveRange(context.InventoryItems);
        context.Suppliers.RemoveRange(context.Suppliers);
        context.Locations.RemoveRange(context.Locations);

        await context.SaveChangesAsync();
    }

    private static async Task SeedLocationsAsync(InventoryDbContext context)
    {
        var locations = new List<Location>
        {
            new Location
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "WH-MAIN",
                Name = "Main Warehouse",
                Description = "Primary storage warehouse",
                LocationType = LocationType.Warehouse,
                Address = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                MaxCapacity = 10000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Location
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "WH-EAST",
                Name = "East Wing",
                Description = "East wing storage area",
                LocationType = LocationType.Warehouse,
                Address = "456 East Ave",
                City = "Springfield",
                State = "IL",
                ZipCode = "62702",
                MaxCapacity = 5000,
                ParentLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Location
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "STORE-001",
                Name = "Main Store",
                Description = "Retail store location",
                LocationType = LocationType.Store,
                Address = "789 Retail Blvd",
                City = "Springfield",
                State = "IL",
                ZipCode = "62703",
                MaxCapacity = 2000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await context.Locations.AddRangeAsync(locations);
    }

    private static async Task SeedSuppliersAsync(InventoryDbContext context)
    {
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Code = "SUP-001",
                Name = "ABC Electronics",
                Description = "Electronic components supplier",
                ContactPerson = "John Doe",
                Email = "john@abcelectronics.com",
                Phone = "555-0101",
                Address = "100 Supplier St",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Supplier
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Code = "SUP-002",
                Name = "XYZ Manufacturing",
                Description = "Manufacturing equipment supplier",
                ContactPerson = "Jane Smith",
                Email = "jane@xyzmanufacturing.com",
                Phone = "555-0102",
                Address = "200 Supplier Ave",
                City = "Detroit",
                State = "MI",
                ZipCode = "48201",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
    }

    private static async Task SeedInventoryItemsAsync(InventoryDbContext context)
    {
        var items = new List<InventoryItem>
        {
            new InventoryItem
            {
                Id = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"),
                PartNumber = "CPU-I7-10700K",
                Description = "Intel Core i7-10700K Processor",
                Barcode = "123456789012",
                Category = "Electronics",
                SubCategory = "Processors",
                UnitOfMeasure = "Each",
                CurrentStock = 25,
                ReservedStock = 5,
                MinimumStock = 10,
                MaximumStock = 100,
                StandardCost = 350.00m,
                SellingPrice = 450.00m,
                LocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SupplierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                IsActive = true,
                LastMovement = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new InventoryItem
            {
                Id = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb"),
                PartNumber = "RAM-16GB-DDR4",
                Description = "16GB DDR4 RAM Module",
                Barcode = "123456789013",
                Category = "Electronics",
                SubCategory = "Memory",
                UnitOfMeasure = "Each",
                CurrentStock = 8,
                ReservedStock = 2,
                MinimumStock = 15,
                MaximumStock = 50,
                StandardCost = 85.00m,
                SellingPrice = 120.00m,
                LocationId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SupplierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                IsActive = true,
                LastMovement = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new InventoryItem
            {
                Id = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc"),
                PartNumber = "SSD-1TB-NVME",
                Description = "1TB NVMe SSD Drive",
                Barcode = "123456789014",
                Category = "Electronics",
                SubCategory = "Storage",
                UnitOfMeasure = "Each",
                CurrentStock = 45,
                ReservedStock = 0,
                MinimumStock = 20,
                MaximumStock = 80,
                StandardCost = 120.00m,
                SellingPrice = 180.00m,
                LocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SupplierId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                IsActive = true,
                LastMovement = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new InventoryItem
            {
                Id = Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd"),
                PartNumber = "MB-Z490-AORUS",
                Description = "Z490 AORUS Motherboard",
                Barcode = "123456789015",
                Category = "Electronics",
                SubCategory = "Motherboards",
                UnitOfMeasure = "Each",
                CurrentStock = 3,
                ReservedStock = 1,
                MinimumStock = 8,
                MaximumStock = 25,
                StandardCost = 280.00m,
                SellingPrice = 380.00m,
                LocationId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                SupplierId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                IsActive = true,
                LastMovement = DateTime.UtcNow.AddHours(-12),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddHours(-6),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await context.InventoryItems.AddRangeAsync(items);
    }

    private static async Task SeedInventoryTransactionsAsync(InventoryDbContext context)
    {
        var transactions = new List<InventoryTransaction>
        {
            new InventoryTransaction
            {
                Id = Guid.Parse("eeeeeeee-5555-5555-5555-eeeeeeeeeeee"),
                TransactionNumber = "REC-001",
                Type = TransactionType.Receipt,
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                InventoryItemId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"),
                DestinationLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Quantity = 50,
                UnitCost = 350.00m,
                ReferenceNumber = "PO-2024-001",
                ReferenceType = "Purchase Order",
                Notes = "Initial stock receipt",
                PerformedBy = "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("ffffffff-6666-6666-6666-ffffffffffff"),
                TransactionNumber = "ISS-001",
                Type = TransactionType.Issue,
                TransactionDate = DateTime.UtcNow.AddDays(-2),
                InventoryItemId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"),
                SourceLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Quantity = 25,
                UnitCost = 350.00m,
                ReferenceNumber = "SO-2024-001",
                ReferenceType = "Sales Order",
                Notes = "Customer order fulfillment",
                PerformedBy = "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("gggggggg-7777-7777-7777-gggggggggggg"),
                TransactionNumber = "TRF-001",
                Type = TransactionType.Transfer,
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                InventoryItemId = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc"),
                SourceLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                DestinationLocationId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Quantity = 10,
                UnitCost = 120.00m,
                ReferenceNumber = "TRF-2024-001",
                ReferenceType = "Stock Transfer",
                Notes = "Rebalancing inventory",
                PerformedBy = "System",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await context.InventoryTransactions.AddRangeAsync(transactions);
    }

    /// <summary>
    /// Gets test data constants for use in tests
    /// </summary>
    public static class TestData
    {
        public static readonly Guid MainWarehouseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid EastWingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid MainStoreId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public static readonly Guid AbcElectronicsId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid XyzManufacturingId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        public static readonly Guid CpuItemId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
        public static readonly Guid RamItemId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");
        public static readonly Guid SsdItemId = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc");
        public static readonly Guid MotherboardItemId = Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd");

        public static readonly string CpuBarcode = "123456789012";
        public static readonly string RamBarcode = "123456789013";
        public static readonly string SsdBarcode = "123456789014";
        public static readonly string MotherboardBarcode = "123456789015";
    }
}
