using FluentValidation;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Application.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTracking.Inventory.Application.Validators;

/// <summary>
/// Validator for creating suppliers
/// </summary>
public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    private readonly ISupplierRepository _supplierRepository;

    public CreateSupplierDtoValidator(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Supplier code is required")
            .MaximumLength(20).WithMessage("Supplier code cannot exceed 20 characters")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Supplier code can only contain uppercase letters, numbers, hyphens, and underscores")
            .MustAsync(BeUniqueSupplierCode).WithMessage("Supplier code already exists");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.ContactTitle)
            .MaximumLength(50).WithMessage("Contact title cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactTitle));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone2)
            .MaximumLength(20).WithMessage("Secondary phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone2));

        RuleFor(x => x.Email2)
            .EmailAddress().WithMessage("Invalid secondary email address format")
            .MaximumLength(100).WithMessage("Secondary email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email2));

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

        RuleFor(x => x.TaxId)
            .MaximumLength(20).WithMessage("Tax ID cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(50).WithMessage("Payment terms cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be a valid 3-letter ISO code")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.LeadTimeDays)
            .InclusiveBetween(0, 365).WithMessage("Lead time must be between 0 and 365 days")
            .When(x => x.LeadTimeDays.HasValue);

        RuleFor(x => x.MinimumOrderQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order quantity cannot be negative")
            .When(x => x.MinimumOrderQuantity.HasValue);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Business rule: at least one contact method
        RuleFor(x => x)
            .Must(HaveContactMethod)
            .WithMessage("At least one contact method (phone or email) must be provided");
    }

    private async Task<bool> BeUniqueSupplierCode(string code, CancellationToken cancellationToken)
    {
        return !await _supplierRepository.ExistsByCodeAsync(code, cancellationToken);
    }

    private bool HaveContactMethod(CreateSupplierDto dto)
    {
        return !string.IsNullOrEmpty(dto.Phone) ||
               !string.IsNullOrEmpty(dto.Email) ||
               !string.IsNullOrEmpty(dto.Phone2) ||
               !string.IsNullOrEmpty(dto.Email2);
    }
}

/// <summary>
/// Validator for updating suppliers
/// </summary>
public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.ContactTitle)
            .MaximumLength(50).WithMessage("Contact title cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactTitle));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone2)
            .MaximumLength(20).WithMessage("Secondary phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone2));

        RuleFor(x => x.Email2)
            .EmailAddress().WithMessage("Invalid secondary email address format")
            .MaximumLength(100).WithMessage("Secondary email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email2));

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

        RuleFor(x => x.TaxId)
            .MaximumLength(20).WithMessage("Tax ID cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(50).WithMessage("Payment terms cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be a valid 3-letter ISO code")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.LeadTimeDays)
            .InclusiveBetween(0, 365).WithMessage("Lead time must be between 0 and 365 days")
            .When(x => x.LeadTimeDays.HasValue);

        RuleFor(x => x.MinimumOrderQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order quantity cannot be negative")
            .When(x => x.MinimumOrderQuantity.HasValue);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for supplier queries
/// </summary>
public class SupplierQueryDtoValidator : AbstractValidator<SupplierQueryDto>
{
    public SupplierQueryDtoValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5).WithMessage("Minimum rating must be between 1 and 5")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Country filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.MaxLeadTime)
            .InclusiveBetween(0, 365).WithMessage("Maximum lead time must be between 0 and 365 days")
            .When(x => x.MaxLeadTime.HasValue);
    }
}

/// <summary>
/// Validator for transaction queries
/// </summary>
public class InventoryTransactionQueryDtoValidator : AbstractValidator<InventoryTransactionQueryDto>
{
    public InventoryTransactionQueryDtoValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.TransactionType)
            .Must(BeValidTransactionType).WithMessage("Invalid transaction type filter")
            .When(x => !string.IsNullOrEmpty(x.TransactionType));

        RuleFor(x => x.Status)
            .Must(BeValidTransactionStatus).WithMessage("Invalid transaction status filter")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.InitiatedBy)
            .MaximumLength(100).WithMessage("User filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.InitiatedBy));

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.MinQuantity)
            .GreaterThan(0).WithMessage("Minimum quantity filter must be greater than zero")
            .When(x => x.MinQuantity.HasValue);

        RuleFor(x => x.MaxQuantity)
            .GreaterThan(0).WithMessage("Maximum quantity filter must be greater than zero")
            .When(x => x.MaxQuantity.HasValue);

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date cannot be after end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        // Cross-field validation for quantity filters
        RuleFor(x => x)
            .Must(HaveValidQuantityRange)
            .WithMessage("Minimum quantity cannot be greater than maximum quantity")
            .When(x => x.MinQuantity.HasValue && x.MaxQuantity.HasValue);
    }

    private bool BeValidTransactionType(string transactionType)
    {
        var validTypes = new[] { "Receipt", "Issue", "Transfer", "Adjustment", "CycleCount", "Return" };
        return validTypes.Contains(transactionType);
    }

    private bool BeValidTransactionStatus(string status)
    {
        var validStatuses = new[] { "Pending", "Approved", "Processing", "Completed", "Cancelled", "Failed" };
        return validStatuses.Contains(status);
    }

    private bool HaveValidQuantityRange(InventoryTransactionQueryDto query)
    {
        return !query.MinQuantity.HasValue || !query.MaxQuantity.HasValue ||
               query.MinQuantity.Value <= query.MaxQuantity.Value;
    }
}
