-- Device Tracking Inventory System - Database Setup Script
-- This script creates the database and initial schema structure
-- Compatible with SQL Server 2019+

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DeviceTracking_Inventory')
BEGIN
    CREATE DATABASE [DeviceTracking_Inventory]
    ON (
        NAME = 'DeviceTracking_Inventory',
        FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\DeviceTracking_Inventory.mdf',
        SIZE = 100MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 10MB
    )
    LOG ON (
        NAME = 'DeviceTracking_Inventory_log',
        FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\DeviceTracking_Inventory_log.ldf',
        SIZE = 50MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 10MB
    );
    PRINT 'Database DeviceTracking_Inventory created successfully.';
END
ELSE
BEGIN
    PRINT 'Database DeviceTracking_Inventory already exists.';
END
GO

USE [DeviceTracking_Inventory];
GO

-- Create Inventory schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
BEGIN
    EXEC('CREATE SCHEMA [Inventory]');
    PRINT 'Schema [Inventory] created successfully.';
END
ELSE
BEGIN
    PRINT 'Schema [Inventory] already exists.';
END
GO

-- Grant permissions to application user (adjust username as needed)
-- This assumes the application connects with integrated security
-- For production, create a specific application user

PRINT 'Database and schema setup complete.';
PRINT 'Next steps:';
PRINT '1. Run 02_CreateSchema.sql to create tables';
PRINT '2. Run 03_CreateIndexes.sql to create indexes';
PRINT '3. Run 04_SeedData.sql to populate initial data';
GO
