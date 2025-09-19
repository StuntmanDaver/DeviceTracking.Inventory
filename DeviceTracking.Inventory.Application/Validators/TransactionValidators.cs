using FluentValidation;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Application.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTracking.Inventory.Application.Validators;

/// <summary>
/// Validator for creating receipt transactions
/// </summary>
public class CreateReceiptDtoValidator : AbstractValidator<CreateReceiptDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ISupplierRepository _supplierRepository;

    public CreateReceiptDtoValidator(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository,
        ISupplierRepository supplierRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));

        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item is required")
            .MustAsync(ItemExists).WithMessage("Inventory item does not exist");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required")
            .MustAsync(LocationExists).WithMessage("Location does not exist");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Quantity cannot exceed 1,000,000 units");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Unit cost can have at most 2 decimal places")
            .When(x => x.UnitCost.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50).WithMessage("Reference type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));

        RuleFor(x => x.SupplierId)
            .MustAsync(SupplierExists).WithMessage("Supplier does not exist")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        return item != null;
    }

    private async Task<bool> LocationExists(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        return location != null;
    }

    private async Task<bool> SupplierExists(Guid? supplierId, CancellationToken cancellationToken)
    {
        if (!supplierId.HasValue) return true;
        var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value, cancellationToken);
        return supplier != null;
    }
}

/// <summary>
/// Validator for creating issue transactions
/// </summary>
public class CreateIssueDtoValidator : AbstractValidator<CreateIssueDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;

    public CreateIssueDtoValidator(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item is required")
            .MustAsync(ItemExists).WithMessage("Inventory item does not exist");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required")
            .MustAsync(LocationExists).WithMessage("Location does not exist");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100000).WithMessage("Quantity cannot exceed 100,000 units");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50).WithMessage("Reference type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rule: sufficient stock
        RuleFor(x => x)
            .MustAsync(HaveSufficientStock)
            .WithMessage("Insufficient stock for this issue transaction");
    }

    private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        return item != null;
    }

    private async Task<bool> LocationExists(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        return location != null;
    }

    private async Task<bool> HaveSufficientStock(CreateIssueDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null) return false;

        var availableStock = item.CurrentStock - item.ReservedStock;
        return availableStock >= dto.Quantity;
    }
}

/// <summary>
/// Validator for creating transfer transactions
/// </summary>
public class CreateTransferDtoValidator : AbstractValidator<CreateTransferDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;

    public CreateTransferDtoValidator(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item is required")
            .MustAsync(ItemExists).WithMessage("Inventory item does not exist");

        RuleFor(x => x.SourceLocationId)
            .NotEmpty().WithMessage("Source location is required")
            .MustAsync(LocationExists).WithMessage("Source location does not exist");

        RuleFor(x => x.DestinationLocationId)
            .NotEmpty().WithMessage("Destination location is required")
            .MustAsync(LocationExists).WithMessage("Destination location does not exist")
            .NotEqual(x => x.SourceLocationId).WithMessage("Source and destination locations cannot be the same");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100000).WithMessage("Quantity cannot exceed 100,000 units");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50).WithMessage("Reference type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rule: sufficient stock at source
        RuleFor(x => x)
            .MustAsync(HaveSufficientStockAtSource)
            .WithMessage("Insufficient stock at source location for transfer");
    }

    private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        return item != null;
    }

    private async Task<bool> LocationExists(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        return location != null;
    }

    private async Task<bool> HaveSufficientStockAtSource(CreateTransferDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null) return false;

        // Check if item is at the source location
        if (item.LocationId != dto.SourceLocationId) return false;

        var availableStock = item.CurrentStock - item.ReservedStock;
        return availableStock >= dto.Quantity;
    }
}

/// <summary>
/// Validator for creating adjustment transactions
/// </summary>
public class CreateAdjustmentDtoValidator : AbstractValidator<CreateAdjustmentDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;

    public CreateAdjustmentDtoValidator(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item is required")
            .MustAsync(ItemExists).WithMessage("Inventory item does not exist");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required")
            .MustAsync(LocationExists).WithMessage("Location does not exist");

        RuleFor(x => x.QuantityAdjustment)
            .NotEqual(0).WithMessage("Quantity adjustment cannot be zero");

        RuleFor(x => x.AdjustmentReason)
            .NotEmpty().WithMessage("Adjustment reason is required")
            .MaximumLength(200).WithMessage("Adjustment reason cannot exceed 200 characters");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Unit cost can have at most 2 decimal places")
            .When(x => x.UnitCost.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rule: prevent negative stock for positive adjustments
        RuleFor(x => x)
            .MustAsync(NotExceedReasonableLimits)
            .WithMessage("Adjustment amount seems unreasonable");
    }

    private async Task<bool> ItemExists(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
        return item != null;
    }

    private async Task<bool> LocationExists(Guid locationId, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        return location != null;
    }

    private async Task<bool> NotExceedReasonableLimits(CreateAdjustmentDto dto, CancellationToken cancellationToken)
    {
        var item = await _inventoryItemRepository.GetByIdAsync(dto.InventoryItemId, cancellationToken);
        if (item == null) return false;

        var adjustment = Math.Abs(dto.QuantityAdjustment);

        // Don't allow adjustments larger than current stock * 2 for negative adjustments
        if (dto.QuantityAdjustment < 0 && adjustment > item.CurrentStock * 2)
        {
            return false;
        }

        // Don't allow adjustments larger than 10,000 units for positive adjustments
        if (dto.QuantityAdjustment > 0 && adjustment > 10000)
        {
            return false;
        }

        return true;
    }
}
