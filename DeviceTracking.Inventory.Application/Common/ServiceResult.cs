using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceTracking.Inventory.Application.Common;

/// <summary>
/// Generic service result for operations that may succeed or fail
/// </summary>
public class ServiceResult<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The result data (if successful)
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Error message (if failed)
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Validation errors (if failed)
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; private set; }

    /// <summary>
    /// Success message
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Private constructor - use factory methods
    /// </summary>
    private ServiceResult() { }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static ServiceResult<T> Success(T data, string? message = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Create a successful result without data
    /// </summary>
    public static ServiceResult<T> Success(string? message = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Message = message
        };
    }

    /// <summary>
    /// Create a failed result with error message
    /// </summary>
    public static ServiceResult<T> Failure(string error)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Error = error
        };
    }

    /// <summary>
    /// Create a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(Dictionary<string, string[]> errors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Errors = errors,
            Error = "Validation failed"
        };
    }

    /// <summary>
    /// Create a failed result with error message and validation errors
    /// </summary>
    public static ServiceResult<T> Failure(string error, Dictionary<string, string[]> errors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Error = error,
            Errors = errors
        };
    }

    /// <summary>
    /// Convert to another result type
    /// </summary>
    public ServiceResult<TOut> To<TOut>()
    {
        if (IsSuccess)
        {
            return ServiceResult<TOut>.Success(Data != null ? (TOut)(object)Data : default, Message);
        }

        return ServiceResult<TOut>.Failure(Error ?? "Operation failed", Errors?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    /// <summary>
    /// Get validation errors as a flat list
    /// </summary>
    public IEnumerable<string> GetValidationErrors()
    {
        if (Errors == null) return Enumerable.Empty<string>();

        return Errors.SelectMany(kvp => kvp.Value);
    }
}

/// <summary>
/// Non-generic service result for operations without data
/// </summary>
public class ServiceResult : ServiceResult<object>
{
    /// <summary>
    /// Create a successful result
    /// </summary>
    public static ServiceResult Success(string? message = null)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static ServiceResult Failure(string error)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    /// <summary>
    /// Create a failed result with validation errors
    /// </summary>
    public static ServiceResult Failure(Dictionary<string, string[]> errors)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            Errors = errors,
            Error = "Validation failed"
        };
    }
}

/// <summary>
/// Common error messages and codes
/// </summary>
public static class ErrorMessages
{
    // Not found errors
    public const string InventoryItemNotFound = "Inventory item not found";
    public const string LocationNotFound = "Location not found";
    public const string SupplierNotFound = "Supplier not found";
    public const string TransactionNotFound = "Transaction not found";

    // Validation errors
    public const string InvalidBarcode = "Invalid barcode format";
    public const string DuplicateBarcode = "Barcode already exists";
    public const string DuplicatePartNumber = "Part number already exists";
    public const string DuplicateLocationCode = "Location code already exists";
    public const string DuplicateSupplierCode = "Supplier code already exists";
    public const string InvalidStockLevel = "Invalid stock level";
    public const string InsufficientStock = "Insufficient stock for operation";

    // Business rule errors
    public const string CannotDeleteActiveItem = "Cannot delete item with active transactions";
    public const string CannotDeleteLocationWithItems = "Cannot delete location containing inventory items";
    public const string InvalidTransactionState = "Invalid transaction state for operation";
    public const string TransactionAlreadyProcessed = "Transaction has already been processed";

    // Permission errors
    public const string InsufficientPermissions = "Insufficient permissions for operation";
    public const string UnauthorizedAccess = "Unauthorized access";

    // System errors
    public const string DatabaseError = "Database operation failed";
    public const string ExternalServiceError = "External service error";
    public const string ConfigurationError = "Configuration error";
}

/// <summary>
/// Common success messages
/// </summary>
public static class SuccessMessages
{
    public const string ItemCreated = "Inventory item created successfully";
    public const string ItemUpdated = "Inventory item updated successfully";
    public const string ItemDeleted = "Inventory item deleted successfully";
    public const string StockUpdated = "Stock level updated successfully";
    public const string TransactionRecorded = "Transaction recorded successfully";
    public const string TransactionApproved = "Transaction approved successfully";
    public const string TransactionProcessed = "Transaction processed successfully";
    public const string LocationCreated = "Location created successfully";
    public const string LocationUpdated = "Location updated successfully";
    public const string SupplierCreated = "Supplier created successfully";
    public const string SupplierUpdated = "Supplier updated successfully";
}
