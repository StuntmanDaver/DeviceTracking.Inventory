using FluentValidation;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Application.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTracking.Inventory.Application.Validators;

/// <summary>
/// Validator for creating inventory items
/// </summary>
public class CreateInventoryItemDtoValidator : AbstractValidator<CreateInventoryItemDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ISupplierRepository _supplierRepository;

    public CreateInventoryItemDtoValidator(
        IInventoryItemRepository inventoryItemRepository,
        ILocationRepository locationRepository,
        ISupplierRepository supplierRepository)
    {
        _inventoryItemRepository = inventoryItemRepository ?? throw new ArgumentNullException(nameof(inventoryItemRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));

        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required")
            .MaximumLength(50).WithMessage("Part number cannot exceed 50 characters")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Part number can only contain letters, numbers, hyphens, and underscores")
            .MustAsync(BeUniquePartNumber).WithMessage("Part number already exists");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("Barcode is required")
            .MaximumLength(100).WithMessage("Barcode cannot exceed 100 characters")
            .MustAsync(BeUniqueBarcode).WithMessage("Barcode already exists");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

        RuleFor(x => x.SubCategory)
            .MaximumLength(50).WithMessage("Sub-category cannot exceed 50 characters");

        RuleFor(x => x.UnitOfMeasure)
            .MaximumLength(20).WithMessage("Unit of measure cannot exceed 20 characters");

        RuleFor(x => x.CurrentStock)
            .GreaterThanOrEqualTo(0).WithMessage("Current stock cannot be negative");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative");

        RuleFor(x => x.MaximumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock cannot be negative");

        RuleFor(x => x.MinimumStock)
            .LessThanOrEqualTo(x => x.MaximumStock)
            .When(x => x.MinimumStock.HasValue && x.MaximumStock.HasValue)
            .WithMessage("Minimum stock cannot be greater than maximum stock");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cost cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Standard cost can have at most 2 decimal places");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Selling price can have at most 2 decimal places");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required")
            .MustAsync(LocationExists).WithMessage("Location does not exist");

        RuleFor(x => x.SupplierId)
            .MustAsync(SupplierExists).WithMessage("Supplier does not exist")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }

    private async Task<bool> BeUniquePartNumber(string partNumber, CancellationToken cancellationToken)
    {
        return !await _inventoryItemRepository.ExistsByPartNumberAsync(partNumber, cancellationToken);
    }

    private async Task<bool> BeUniqueBarcode(string barcode, CancellationToken cancellationToken)
    {
        var existingItem = await _inventoryItemRepository.GetByBarcodeAsync(barcode, cancellationToken);
        return existingItem == null;
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
/// Validator for updating inventory items
/// </summary>
public class UpdateInventoryItemDtoValidator : AbstractValidator<UpdateInventoryItemDto>
{
    public UpdateInventoryItemDtoValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.SubCategory)
            .MaximumLength(50).WithMessage("Sub-category cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.SubCategory));

        RuleFor(x => x.UnitOfMeasure)
            .MaximumLength(20).WithMessage("Unit of measure cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.UnitOfMeasure));

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative")
            .When(x => x.MinimumStock.HasValue);

        RuleFor(x => x.MaximumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock cannot be negative")
            .When(x => x.MaximumStock.HasValue);

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cost cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Standard cost can have at most 2 decimal places")
            .When(x => x.StandardCost.HasValue);

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price cannot be negative")
            .ScalePrecision(2, 18).WithMessage("Selling price can have at most 2 decimal places")
            .When(x => x.SellingPrice.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Cross-field validation
        RuleFor(x => x)
            .Must(HaveValidStockLevels)
            .WithMessage("Minimum stock cannot be greater than maximum stock")
            .When(x => x.MinimumStock.HasValue && x.MaximumStock.HasValue);
    }

    private bool HaveValidStockLevels(UpdateInventoryItemDto dto)
    {
        return !dto.MinimumStock.HasValue || !dto.MaximumStock.HasValue ||
               dto.MinimumStock.Value <= dto.MaximumStock.Value;
    }
}

/// <summary>
/// Validator for inventory item queries
/// </summary>
public class InventoryItemQueryDtoValidator : AbstractValidator<InventoryItemQueryDto>
{
    public InventoryItemQueryDtoValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.SubCategory)
            .MaximumLength(50).WithMessage("Sub-category cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.SubCategory));

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock filter cannot be negative")
            .When(x => x.MinStock.HasValue);

        RuleFor(x => x.MaxStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock filter cannot be negative")
            .When(x => x.MaxStock.HasValue);

        RuleFor(x => x.MinCost)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum cost filter cannot be negative")
            .When(x => x.MinCost.HasValue);

        RuleFor(x => x.MaxCost)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum cost filter cannot be negative")
            .When(x => x.MaxCost.HasValue);

        // Cross-field validation for stock filters
        RuleFor(x => x)
            .Must(HaveValidStockRange)
            .WithMessage("Minimum stock cannot be greater than maximum stock")
            .When(x => x.MinStock.HasValue && x.MaxStock.HasValue);

        // Cross-field validation for cost filters
        RuleFor(x => x)
            .Must(HaveValidCostRange)
            .WithMessage("Minimum cost cannot be greater than maximum cost")
            .When(x => x.MinCost.HasValue && x.MaxCost.HasValue);
    }

    private bool HaveValidStockRange(InventoryItemQueryDto query)
    {
        return !query.MinStock.HasValue || !query.MaxStock.HasValue ||
               query.MinStock.Value <= query.MaxStock.Value;
    }

    private bool HaveValidCostRange(InventoryItemQueryDto query)
    {
        return !query.MinCost.HasValue || !query.MaxCost.HasValue ||
               query.MinCost.Value <= query.MaxCost.Value;
    }
}
