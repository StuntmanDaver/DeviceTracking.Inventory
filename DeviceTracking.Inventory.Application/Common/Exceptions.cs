using System;
using System.Collections.Generic;

namespace DeviceTracking.Inventory.Application.Common;

/// <summary>
/// Base exception for business logic errors
/// </summary>
public abstract class BusinessException : Exception
{
    /// <summary>
    /// Error code for the exception
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code to return
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public object? Details { get; }

    protected BusinessException(string message, string errorCode, int statusCode, object? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
    }

    protected BusinessException(string message, string errorCode, int statusCode, Exception innerException, object? details = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : BusinessException
{
    /// <summary>
    /// Validation errors
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR", 400, errors)
    {
        ValidationErrors = errors;
    }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message, "VALIDATION_ERROR", 400, errors)
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Exception for resource not found errors
/// </summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' not found", "RESOURCE_NOT_FOUND", 404,
              new { ResourceType = resourceType, ResourceId = resourceId })
    {
    }

    public NotFoundException(string message)
        : base(message, "RESOURCE_NOT_FOUND", 404)
    {
    }
}

/// <summary>
/// Exception for unauthorized access
/// </summary>
public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED", 401)
    {
    }

    public UnauthorizedException(string resource, string action)
        : base($"Unauthorized to {action} {resource}", "UNAUTHORIZED", 401,
              new { Resource = resource, Action = action })
    {
    }
}

/// <summary>
/// Exception for forbidden operations
/// </summary>
public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "FORBIDDEN", 403)
    {
    }

    public ForbiddenException(string resource, string action)
        : base($"Access forbidden for {action} on {resource}", "FORBIDDEN", 403,
              new { Resource = resource, Action = action })
    {
    }
}

/// <summary>
/// Exception for conflict errors
/// </summary>
public class ConflictException : BusinessException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", 409)
    {
    }

    public ConflictException(string resource, object existingId)
        : base($"{resource} already exists", "CONFLICT", 409,
              new { Resource = resource, ExistingId = existingId })
    {
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : BusinessException
{
    public BusinessRuleException(string message, string ruleName)
        : base(message, "BUSINESS_RULE_VIOLATION", 400,
              new { RuleName = ruleName })
    {
    }

    public BusinessRuleException(string message, string ruleName, object details)
        : base(message, "BUSINESS_RULE_VIOLATION", 400, details)
    {
    }
}

/// <summary>
/// Exception for external service errors
/// </summary>
public class ExternalServiceException : BusinessException
{
    public ExternalServiceException(string serviceName, string message)
        : base($"External service error in {serviceName}: {message}", "EXTERNAL_SERVICE_ERROR", 502,
              new { ServiceName = serviceName })
    {
    }

    public ExternalServiceException(string serviceName, Exception innerException)
        : base($"External service error in {serviceName}: {innerException.Message}", "EXTERNAL_SERVICE_ERROR", 502, innerException,
              new { ServiceName = serviceName })
    {
    }
}

/// <summary>
/// Exception for concurrency conflicts
/// </summary>
public class ConcurrencyException : BusinessException
{
    public ConcurrencyException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' was modified by another user", "CONCURRENCY_CONFLICT", 409,
              new { ResourceType = resourceType, ResourceId = resourceId })
    {
    }
}

/// <summary>
/// Exception for rate limiting
/// </summary>
public class RateLimitException : BusinessException
{
    public RateLimitException(int limit, TimeSpan window)
        : base($"Rate limit exceeded. Maximum {limit} requests per {window.TotalMinutes} minutes", "RATE_LIMIT_EXCEEDED", 429,
              new { Limit = limit, WindowMinutes = window.TotalMinutes })
    {
    }
}
