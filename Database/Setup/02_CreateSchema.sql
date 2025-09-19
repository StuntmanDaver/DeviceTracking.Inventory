-- Device Tracking Inventory System - Schema Creation Script
-- Creates tables and constraints for the Inventory schema
-- Compatible with SQL Server 2019+

USE [DeviceTracking_Inventory];
GO

-- Create Locations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locations' AND xtype='U')
BEGIN
    CREATE TABLE [Inventory].[Locations] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Code] NVARCHAR(20) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [LocationType] NVARCHAR(20) NOT NULL DEFAULT 'Warehouse',
        [ParentLocationId] UNIQUEIDENTIFIER NULL,
        [Address] NVARCHAR(100) NULL,
        [City] NVARCHAR(50) NULL,
        [State] NVARCHAR(50) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(50) NULL,
        [ContactPerson] NVARCHAR(100) NULL,
        [ContactPhone] NVARCHAR(20) NULL,
        [ContactEmail] NVARCHAR(100) NULL,
        [MaxCapacity] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,

        CONSTRAINT [CK_Locations_LocationType] CHECK ([LocationType] IN ('Warehouse', 'Production', 'Customer', 'Supplier', 'Transit')),
        CONSTRAINT [FK_Locations_ParentLocation] FOREIGN KEY ([ParentLocationId]) REFERENCES [Inventory].[Locations]([Id])
    );

    CREATE UNIQUE INDEX [IX_Locations_Code] ON [Inventory].[Locations]([Code]);
    CREATE INDEX [IX_Locations_Parent_Active] ON [Inventory].[Locations]([ParentLocationId], [IsActive]);

    PRINT 'Table [Inventory].[Locations] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Inventory].[Locations] already exists.';
END
GO

-- Create Suppliers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Suppliers' AND xtype='U')
BEGIN
    CREATE TABLE [Inventory].[Suppliers] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Code] NVARCHAR(20) NOT NULL,
        [CompanyName] NVARCHAR(100) NOT NULL,
        [ContactPerson] NVARCHAR(100) NULL,
        [ContactTitle] NVARCHAR(50) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Phone2] NVARCHAR(20) NULL,
        [Email2] NVARCHAR(100) NULL,
        [Address] NVARCHAR(100) NULL,
        [City] NVARCHAR(50) NULL,
        [State] NVARCHAR(50) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(50) NULL,
        [TaxId] NVARCHAR(20) NULL,
        [PaymentTerms] NVARCHAR(50) NULL,
        [Currency] NVARCHAR(3) NULL DEFAULT 'USD',
        [LeadTimeDays] INT NULL,
        [MinimumOrderQuantity] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [Rating] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,

        CONSTRAINT [CK_Suppliers_Rating] CHECK ([Rating] IS NULL OR [Rating] BETWEEN 1 AND 5),
        CONSTRAINT [CK_Suppliers_LeadTime] CHECK ([LeadTimeDays] IS NULL OR [LeadTimeDays] BETWEEN 0 AND 365),
        CONSTRAINT [CK_Suppliers_MinOrderQty] CHECK ([MinimumOrderQuantity] IS NULL OR [MinimumOrderQuantity] >= 0)
    );

    CREATE UNIQUE INDEX [IX_Suppliers_Code] ON [Inventory].[Suppliers]([Code]);

    PRINT 'Table [Inventory].[Suppliers] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Inventory].[Suppliers] already exists.';
END
GO

-- Create InventoryItems table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InventoryItems' AND xtype='U')
BEGIN
    CREATE TABLE [Inventory].[InventoryItems] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [PartNumber] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NOT NULL,
        [Barcode] NVARCHAR(100) NOT NULL,
        [Category] NVARCHAR(50) NULL,
        [SubCategory] NVARCHAR(50) NULL,
        [UnitOfMeasure] NVARCHAR(20) NOT NULL DEFAULT 'Each',
        [CurrentStock] INT NOT NULL DEFAULT 0,
        [ReservedStock] INT NOT NULL DEFAULT 0,
        [MinimumStock] INT NOT NULL DEFAULT 0,
        [MaximumStock] INT NOT NULL DEFAULT 0,
        [StandardCost] DECIMAL(18,4) NOT NULL DEFAULT 0,
        [SellingPrice] DECIMAL(18,4) NOT NULL DEFAULT 0,
        [LocationId] UNIQUEIDENTIFIER NOT NULL,
        [SupplierId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [LastMovement] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,

        CONSTRAINT [CK_InventoryItems_CurrentStock] CHECK ([CurrentStock] >= 0),
        CONSTRAINT [CK_InventoryItems_ReservedStock] CHECK ([ReservedStock] >= 0),
        CONSTRAINT [CK_InventoryItems_MinStock] CHECK ([MinimumStock] >= 0),
        CONSTRAINT [CK_InventoryItems_MaxStock] CHECK ([MaximumStock] >= 0),
        CONSTRAINT [CK_InventoryItems_Costs] CHECK ([StandardCost] >= 0 AND [SellingPrice] >= 0),
        CONSTRAINT [FK_InventoryItems_Location] FOREIGN KEY ([LocationId]) REFERENCES [Inventory].[Locations]([Id]),
        CONSTRAINT [FK_InventoryItems_Supplier] FOREIGN KEY ([SupplierId]) REFERENCES [Inventory].[Suppliers]([Id]) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX [IX_InventoryItems_PartNumber] ON [Inventory].[InventoryItems]([PartNumber]);
    CREATE UNIQUE INDEX [IX_InventoryItems_Barcode] ON [Inventory].[InventoryItems]([Barcode]);
    CREATE INDEX [IX_InventoryItems_Location_Active_LastMovement] ON [Inventory].[InventoryItems]([LocationId], [IsActive], [LastMovement]);

    PRINT 'Table [Inventory].[InventoryItems] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Inventory].[InventoryItems] already exists.';
END
GO

-- Create InventoryTransactions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InventoryTransactions' AND xtype='U')
BEGIN
    CREATE TABLE [Inventory].[InventoryTransactions] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [TransactionNumber] NVARCHAR(20) NOT NULL,
        [TransactionType] NVARCHAR(20) NOT NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [InventoryItemId] UNIQUEIDENTIFIER NOT NULL,
        [SourceLocationId] UNIQUEIDENTIFIER NULL,
        [DestinationLocationId] UNIQUEIDENTIFIER NULL,
        [Quantity] INT NOT NULL,
        [UnitCost] DECIMAL(18,4) NULL,
        [ReferenceNumber] NVARCHAR(50) NULL,
        [ReferenceType] NVARCHAR(20) NULL,
        [AdjustmentReason] NVARCHAR(100) NULL,
        [InitiatedBy] NVARCHAR(100) NOT NULL,
        [InitiatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ApprovedBy] NVARCHAR(100) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [ProcessedBy] NVARCHAR(100) NULL,
        [ProcessedAt] DATETIME2 NULL,
        [Notes] NVARCHAR(500) NULL,
        [BeforeImage] NVARCHAR(1000) NULL,
        [AfterImage] NVARCHAR(1000) NULL,
        [IsQuickBooksSynced] BIT NOT NULL DEFAULT 0,
        [QuickBooksRefId] NVARCHAR(50) NULL,

        CONSTRAINT [CK_InventoryTransactions_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_InventoryTransactions_UnitCost] CHECK ([UnitCost] IS NULL OR [UnitCost] >= 0),
        CONSTRAINT [CK_InventoryTransactions_Type] CHECK ([TransactionType] IN ('Receipt', 'Issue', 'Transfer', 'Adjustment', 'CountAdjustment')),
        CONSTRAINT [CK_InventoryTransactions_Status] CHECK ([Status] IN ('Pending', 'Approved', 'Processing', 'Completed', 'Cancelled', 'Failed')),
        CONSTRAINT [FK_InventoryTransactions_InventoryItem] FOREIGN KEY ([InventoryItemId]) REFERENCES [Inventory].[InventoryItems]([Id]),
        CONSTRAINT [FK_InventoryTransactions_SourceLocation] FOREIGN KEY ([SourceLocationId]) REFERENCES [Inventory].[Locations]([Id]),
        CONSTRAINT [FK_InventoryTransactions_DestinationLocation] FOREIGN KEY ([DestinationLocationId]) REFERENCES [Inventory].[Locations]([Id])
    );

    CREATE UNIQUE INDEX [IX_InventoryTransactions_Number] ON [Inventory].[InventoryTransactions]([TransactionNumber]);
    CREATE INDEX [IX_InventoryTransactions_Type_Status_ProcessedAt] ON [Inventory].[InventoryTransactions]([TransactionType], [Status], [ProcessedAt]);

    PRINT 'Table [Inventory].[InventoryTransactions] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Inventory].[InventoryTransactions] already exists.';
END
GO

PRINT 'Database schema creation complete.';
PRINT 'Next steps:';
PRINT '1. Run EF Core migrations to ensure compatibility';
PRINT '2. Run 03_CreateIndexes.sql for additional performance indexes';
PRINT '3. Run 04_SeedData.sql to populate initial data';
GO
