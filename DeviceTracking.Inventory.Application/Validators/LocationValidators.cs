using FluentValidation;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Application.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTracking.Inventory.Application.Validators;

/// <summary>
/// Validator for creating locations
/// </summary>
public class CreateLocationDtoValidator : AbstractValidator<CreateLocationDto>
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationDtoValidator(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Location code is required")
            .MaximumLength(20).WithMessage("Location code cannot exceed 20 characters")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Location code can only contain uppercase letters, numbers, hyphens, and underscores")
            .MustAsync(BeUniqueLocationCode).WithMessage("Location code already exists");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MaximumLength(100).WithMessage("Location name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");

        RuleFor(x => x.LocationType)
            .NotEmpty().WithMessage("Location type is required")
            .Must(BeValidLocationType).WithMessage("Invalid location type");

        RuleFor(x => x.ParentLocationId)
            .MustAsync(ParentLocationExists).WithMessage("Parent location does not exist")
            .When(x => x.ParentLocationId.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(100).WithMessage("Address cannot exceed 100 characters");

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
            .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("Invalid postal code format")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Country cannot exceed 50 characters");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email address format")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Maximum capacity must be greater than zero")
            .When(x => x.MaxCapacity.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");

        // Business rule validation
        RuleFor(x => x)
            .MustAsync(HaveValidHierarchy)
            .WithMessage("Invalid location hierarchy")
            .When(x => x.ParentLocationId.HasValue);
    }

    private async Task<bool> BeUniqueLocationCode(string code, CancellationToken cancellationToken)
    {
        return !await _locationRepository.ExistsByCodeAsync(code, cancellationToken);
    }

    private bool BeValidLocationType(string locationType)
    {
        // This would typically use an enum or lookup table
        var validTypes = new[] { "Warehouse", "ProductionFloor", "CustomerSite", "SupplierLocation", "Transit", "Quarantine", "Other" };
        return validTypes.Contains(locationType);
    }

    private async Task<bool> ParentLocationExists(Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return true;
        var parent = await _locationRepository.GetByIdAsync(parentId.Value, cancellationToken);
        return parent != null;
    }

    private async Task<bool> HaveValidHierarchy(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        if (!dto.ParentLocationId.HasValue) return true;

        // Check for circular references and depth limits
        var visited = new HashSet<Guid> { Guid.NewGuid() }; // This would be the new location's ID
        var currentParentId = dto.ParentLocationId.Value;

        var depth = 0;
        while (currentParentId != Guid.Empty && depth < 6) // Max depth check
        {
            if (visited.Contains(currentParentId))
            {
                return false; // Circular reference
            }

            visited.Add(currentParentId);

            var parent = await _locationRepository.GetByIdAsync(currentParentId, cancellationToken);
            if (parent == null)
            {
                return false; // Parent doesn't exist
            }

            currentParentId = parent.ParentLocationId ?? Guid.Empty;
            depth++;
        }

        return depth < 6; // Ensure hierarchy depth is within limits
    }
}

/// <summary>
/// Validator for updating locations
/// </summary>
public class UpdateLocationDtoValidator : AbstractValidator<UpdateLocationDto>
{
    public UpdateLocationDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Location name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LocationType)
            .Must(BeValidLocationType).WithMessage("Invalid location type")
            .When(x => !string.IsNullOrEmpty(x.LocationType));

        RuleFor(x => x.Address)
            .MaximumLength(100).WithMessage("Address cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
            .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("Invalid postal code format")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Country cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email address format")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Maximum capacity must be greater than zero")
            .When(x => x.MaxCapacity.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeValidLocationType(string locationType)
    {
        var validTypes = new[] { "Warehouse", "ProductionFloor", "CustomerSite", "SupplierLocation", "Transit", "Quarantine", "Other" };
        return validTypes.Contains(locationType);
    }
}

/// <summary>
/// Validator for location queries
/// </summary>
public class LocationQueryDtoValidator : AbstractValidator<LocationQueryDto>
{
    public LocationQueryDtoValidator()
    {
        RuleFor(x => x.LocationType)
            .Must(BeValidLocationType).WithMessage("Invalid location type filter")
            .When(x => !string.IsNullOrEmpty(x.LocationType));

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.State));
    }

    private bool BeValidLocationType(string locationType)
    {
        var validTypes = new[] { "Warehouse", "ProductionFloor", "CustomerSite", "SupplierLocation", "Transit", "Quarantine", "Other" };
        return validTypes.Contains(locationType);
    }
}
