using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using DeviceTracking.Inventory.Application.Common;
using DeviceTracking.Inventory.Application.Repositories;
using DeviceTracking.Inventory.Application.Services;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Shared.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace DeviceTracking.Inventory.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for InventoryItemService
/// </summary>
public class InventoryItemServiceTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly Mock<ISupplierRepository> _supplierRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryItemService _sut;
    private readonly Fixture _fixture;

    public InventoryItemServiceTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _locationRepositoryMock = new Mock<ILocationRepository>();
        _supplierRepositoryMock = new Mock<ISupplierRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fixture = new Fixture();

        // Setup AutoMapper mock or create a simple mapper for testing
        var mapper = AutoMapperHelper.CreateMapper();

        _sut = new InventoryItemService(
            _inventoryItemRepositoryMock.Object,
            _locationRepositoryMock.Object,
            _supplierRepositoryMock.Object,
            _unitOfWorkMock.Object,
            mapper);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullInventoryItemRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InventoryItemService(
            null!,
            _locationRepositoryMock.Object,
            _supplierRepositoryMock.Object,
            _unitOfWorkMock.Object,
            AutoMapperHelper.CreateMapper()));
    }

    [Fact]
    public void Constructor_WithNullLocationRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InventoryItemService(
            _inventoryItemRepositoryMock.Object,
            null!,
            _supplierRepositoryMock.Object,
            _unitOfWorkMock.Object,
            AutoMapperHelper.CreateMapper()));
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InventoryItemService(
            _inventoryItemRepositoryMock.Object,
            _locationRepositoryMock.Object,
            _supplierRepositoryMock.Object,
            null!,
            AutoMapperHelper.CreateMapper()));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItem);

        // Act
        var result = await _sut.GetByIdAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(itemId);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnFailResult()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.GetByIdAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Inventory item not found");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithRepositoryException_ShouldReturnFailResult()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedException = new Exception("Database error");

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _sut.GetByIdAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving inventory item");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByBarcodeAsync Tests

    [Fact]
    public async Task GetByBarcodeAsync_WithValidBarcode_ShouldReturnItem()
    {
        // Arrange
        var barcode = "123456789012";
        var expectedItem = _fixture.Build<InventoryItem>()
            .With(x => x.Barcode, barcode)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItem);

        // Act
        var result = await _sut.GetByBarcodeAsync(barcode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Barcode.Should().Be(barcode);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByBarcodeAsync_WithNonExistentBarcode_ShouldReturnFailResult()
    {
        // Arrange
        var barcode = "999999999999";

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.GetByBarcodeAsync(barcode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Inventory item not found for the specified barcode");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new InventoryItemQueryDto { Page = 1, PageSize = 10 };
        var items = _fixture.CreateMany<InventoryItem>(5).ToList();
        var totalCount = 25;

        _inventoryItemRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        _inventoryItemRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(5);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalCount.Should().Be(25);

        _inventoryItemRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WithRepositoryException_ShouldReturnFailResult()
    {
        // Arrange
        var query = new InventoryItemQueryDto { Page = 1, PageSize = 10 };
        var expectedException = new Exception("Database error");

        _inventoryItemRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving inventory items");

        _inventoryItemRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateItemAndReturnSuccess()
    {
        // Arrange
        var dto = _fixture.Build<CreateInventoryItemDto>()
            .With(x => x.PartNumber, "TEST-001")
            .With(x => x.Description, "Test Item")
            .Create();

        var createdItem = _fixture.Build<InventoryItem>()
            .With(x => x.PartNumber, dto.PartNumber)
            .With(x => x.Description, dto.Description)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PartNumber.Should().Be(dto.PartNumber);
        result.Data.Description.Should().Be(dto.Description);

        _inventoryItemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithRepositoryException_ShouldReturnFailResult()
    {
        // Arrange
        var dto = _fixture.Create<CreateInventoryItemDto>();
        var expectedException = new Exception("Database error");

        _inventoryItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating inventory item");

        _inventoryItemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateItemAndReturnSuccess()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var dto = _fixture.Build<UpdateInventoryItemDto>()
            .With(x => x.Description, "Updated Description")
            .Create();

        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(itemId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldReturnFailResult()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var dto = _fixture.Create<UpdateInventoryItemDto>();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.UpdateAsync(itemId, dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Inventory item not found");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteItemAndReturnSuccess()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .With(x => x.IsActive, true)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Inventory item deleted successfully");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFailResult()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.DeleteAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Inventory item not found");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Never);
    }

    #endregion

    #region UpdateStockAsync Tests

    [Fact]
    public async Task UpdateStockAsync_WithValidData_ShouldUpdateStockAndReturnSuccess()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantityChange = 10;
        var reason = "Manual adjustment";
        var currentStock = 50;

        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .With(x => x.CurrentStock, currentStock)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateStockAsync(itemId, quantityChange, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CurrentStock.Should().Be(currentStock + quantityChange);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockAsync_WithNegativeResult_ShouldReturnFailResult()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantityChange = -100; // More than current stock
        var reason = "Invalid adjustment";
        var currentStock = 50;

        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .With(x => x.CurrentStock, currentStock)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        // Act
        var result = await _sut.UpdateStockAsync(itemId, quantityChange, reason);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Stock level cannot be negative");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Never);
    }

    #endregion

    #region RecordBarcodeScanAsync Tests

    [Fact]
    public async Task RecordBarcodeScanAsync_WithValidBarcode_ShouldUpdateItemAndReturnSuccess()
    {
        // Arrange
        var barcode = "123456789012";
        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Barcode, barcode)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.RecordBarcodeScanAsync(barcode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Barcode scan recorded successfully");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByBarcodeAsync(barcode, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetLowStockAlertsAsync Tests

    [Fact]
    public async Task GetLowStockAlertsAsync_WithValidThreshold_ShouldReturnAlerts()
    {
        // Arrange
        var threshold = 10;
        var lowStockItems = _fixture.CreateMany<InventoryItem>(3).ToList();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetLowStockItemsAsync(threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowStockItems);

        // Act
        var result = await _sut.GetLowStockAlertsAsync(threshold);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetLowStockItemsAsync(threshold, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task CreateAsync_WithDuplicateBarcode_ShouldHandleGracefully()
    {
        // This test verifies that the service handles potential business rule violations
        // In a real scenario, this would test unique constraint violations
        var dto = _fixture.Build<CreateInventoryItemDto>()
            .With(x => x.Barcode, "DUPLICATE-123")
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("UNIQUE constraint violation"));

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating inventory item");

        _inventoryItemRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockAsync_WithLargeQuantityChange_ShouldHandleSuccessfully()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var largeQuantityChange = 10000;
        var currentStock = 100;

        var existingItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, itemId)
            .With(x => x.CurrentStock, currentStock)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateStockAsync(itemId, largeQuantityChange, "Bulk restock");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CurrentStock.Should().Be(currentStock + largeQuantityChange);
    }

    #endregion
}

/// <summary>
/// Helper class for AutoMapper setup in tests
/// </summary>
public static class AutoMapperHelper
{
    public static AutoMapper.IMapper CreateMapper()
    {
        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            // Add your mapping profiles here
            cfg.CreateMap<CreateInventoryItemDto, InventoryItem>();
            cfg.CreateMap<UpdateInventoryItemDto, InventoryItem>();
            cfg.CreateMap<InventoryItem, InventoryItemDto>();
            cfg.CreateMap<InventoryItem, InventoryItemSummaryDto>();
            // Add other mappings as needed
        });

        return config.CreateMapper();
    }
}
