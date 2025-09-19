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
/// Unit tests for LocationService
/// </summary>
public class LocationServiceTests
{
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LocationService _sut;
    private readonly Fixture _fixture;

    public LocationServiceTests()
    {
        _locationRepositoryMock = new Mock<ILocationRepository>();
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fixture = new Fixture();

        var mapper = AutoMapperHelper.CreateMapper();

        _sut = new LocationService(
            _locationRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            mapper);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLocationRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LocationService(
            null!,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            AutoMapperHelper.CreateMapper()));
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LocationService(
            _locationRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            null!,
            AutoMapperHelper.CreateMapper()));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnLocation()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var expectedLocation = _fixture.Build<Location>()
            .With(x => x.Id, locationId)
            .Create();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLocation);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _sut.GetByIdAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(locationId);

        _locationRepositoryMock.Verify(
            x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnFailResult()
    {
        // Arrange
        var locationId = Guid.NewGuid();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _sut.GetByIdAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Location not found");

        _locationRepositoryMock.Verify(
            x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByCodeAsync Tests

    [Fact]
    public async Task GetByCodeAsync_WithValidCode_ShouldReturnLocation()
    {
        // Arrange
        var locationCode = "WH-001";
        var expectedLocation = _fixture.Build<Location>()
            .With(x => x.Code, locationCode)
            .Create();

        _locationRepositoryMock
            .Setup(x => x.GetByCodeAsync(locationCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLocation);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(expectedLocation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _sut.GetByCodeAsync(locationCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(locationCode);

        _locationRepositoryMock.Verify(
            x => x.GetByCodeAsync(locationCode, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new LocationQueryDto { Page = 1, PageSize = 10 };
        var locations = _fixture.CreateMany<Location>(5).ToList();
        var totalCount = 25;

        _locationRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _locationRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(5);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalCount.Should().Be(25);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateLocationAndReturnSuccess()
    {
        // Arrange
        var dto = _fixture.Build<CreateLocationDto>()
            .With(x => x.Code, "WH-001")
            .With(x => x.Name, "Main Warehouse")
            .Create();

        var createdLocation = _fixture.Build<Location>()
            .With(x => x.Code, dto.Code)
            .With(x => x.Name, dto.Name)
            .Create();

        _locationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(dto.Code);
        result.Data.Name.Should().Be(dto.Name);

        _locationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateLocationAndReturnSuccess()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var dto = _fixture.Build<UpdateLocationDto>()
            .With(x => x.Name, "Updated Warehouse Name")
            .Create();

        var existingLocation = _fixture.Build<Location>()
            .With(x => x.Id, locationId)
            .Create();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLocation);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(locationId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _locationRepositoryMock.Verify(
            x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);

        _locationRepositoryMock.Verify(
            x => x.Update(It.IsAny<Location>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidIdAndNoItems_ShouldSoftDeleteLocationAndReturnSuccess()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var existingLocation = _fixture.Build<Location>()
            .With(x => x.Id, locationId)
            .With(x => x.IsActive, true)
            .Create();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLocation);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>()); // No items at location

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Location deleted successfully");

        _locationRepositoryMock.Verify(
            x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);

        _locationRepositoryMock.Verify(
            x => x.Update(It.IsAny<Location>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithItemsAtLocation_ShouldReturnFailResult()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var existingLocation = _fixture.Build<Location>()
            .With(x => x.Id, locationId)
            .Create();

        var itemsAtLocation = _fixture.CreateMany<InventoryItem>(3).ToList();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLocation);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsAtLocation); // Has items at location

        // Act
        var result = await _sut.DeleteAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Cannot delete location that contains inventory items");

        _locationRepositoryMock.Verify(
            x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()),
            Times.Once);

        _locationRepositoryMock.Verify(
            x => x.Update(It.IsAny<Location>()),
            Times.Never);
    }

    #endregion

    #region GetHierarchyAsync Tests

    [Fact]
    public async Task GetHierarchyAsync_WithValidData_ShouldReturnHierarchy()
    {
        // Arrange
        var locations = new List<Location>
        {
            _fixture.Build<Location>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Code, "ROOT")
                .With(x => x.ParentLocationId, (Guid?)null)
                .Create(),
            _fixture.Build<Location>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Code, "CHILD1")
                .With(x => x.ParentLocationId, Guid.NewGuid())
                .Create()
        };

        _locationRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        // Act
        var result = await _sut.GetHierarchyAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _locationRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_WithValidType_ShouldReturnFilteredLocations()
    {
        // Arrange
        var locationType = "Warehouse";
        var locations = _fixture.CreateMany<Location>(3).ToList();

        _locationRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _sut.GetByTypeAsync(locationType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);

        _locationRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetCapacityUtilizationAsync Tests

    [Fact]
    public async Task GetCapacityUtilizationAsync_WithValidData_ShouldReturnUtilizationData()
    {
        // Arrange
        var locations = _fixture.CreateMany<Location>(2).ToList();
        var items = _fixture.CreateMany<InventoryItem>(5).ToList();

        _locationRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByLocationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        // Act
        var result = await _sut.GetCapacityUtilizationAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);

        _locationRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region TransferItemsAsync Tests

    [Fact]
    public async Task TransferItemsAsync_WithValidData_ShouldTransferItemsSuccessfully()
    {
        // Arrange
        var fromLocationId = Guid.NewGuid();
        var toLocationId = Guid.NewGuid();
        var items = new List<(Guid ItemId, int Quantity)>
        {
            (Guid.NewGuid(), 5),
            (Guid.NewGuid(), 10)
        };
        var reason = "Stock redistribution";

        var inventoryItems = items.Select(item =>
            _fixture.Build<InventoryItem>()
                .With(x => x.Id, item.ItemId)
                .With(x => x.CurrentStock, 100)
                .With(x => x.LocationId, fromLocationId)
                .Create()
        ).ToList();

        foreach (var item in inventoryItems)
        {
            _inventoryItemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
        }

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.TransferItemsAsync(fromLocationId, toLocationId, items, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Successfully transferred 2 items");

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Exactly(2));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransferItemsAsync_WithInsufficientStock_ShouldReturnFailResult()
    {
        // Arrange
        var fromLocationId = Guid.NewGuid();
        var toLocationId = Guid.NewGuid();
        var items = new List<(Guid ItemId, int Quantity)>
        {
            (Guid.NewGuid(), 100) // More than available stock
        };
        var reason = "Invalid transfer";

        var inventoryItem = _fixture.Build<InventoryItem>()
            .With(x => x.Id, items[0].ItemId)
            .With(x => x.CurrentStock, 50) // Less than requested
            .With(x => x.LocationId, fromLocationId)
            .Create();

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(inventoryItem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _sut.TransferItemsAsync(fromLocationId, toLocationId, items, reason);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient stock");

        _inventoryItemRepositoryMock.Verify(
            x => x.GetByIdAsync(inventoryItem.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _inventoryItemRepositoryMock.Verify(
            x => x.Update(It.IsAny<InventoryItem>()),
            Times.Never);
    }

    #endregion
}
