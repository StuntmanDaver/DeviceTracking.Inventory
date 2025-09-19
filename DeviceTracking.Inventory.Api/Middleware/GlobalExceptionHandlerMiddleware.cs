using System;
using System.Net;
using System.Threading.Tasks;
using DeviceTracking.Inventory.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Collections.Generic;

namespace DeviceTracking.Inventory.Api.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error handling
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(exception, context);
        var statusCode = GetStatusCode(exception);

        // Log the exception
        LogException(exception, context, statusCode);

        // Set response properties
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        // Add common headers
        context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
        context.Response.Headers["X-Timestamp"] = DateTime.UtcNow.ToString("O");

        // Write response
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false // Compact JSON for production
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }

    private ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        var instance = $"{context.Request.Method} {context.Request.Path}";
        var traceId = context.TraceIdentifier;

        return exception switch
        {
            BusinessException businessException =>
                ProblemDetails.FromBusinessException(businessException, instance),

            ValidationException validationException =>
                ProblemDetails.FromValidationException(validationException, instance),

            NotFoundException notFoundException =>
                ProblemDetails.FromBusinessException(notFoundException, instance),

            UnauthorizedException unauthorizedException =>
                ProblemDetails.FromBusinessException(unauthorizedException, instance),

            ForbiddenException forbiddenException =>
                ProblemDetails.FromBusinessException(forbiddenException, instance),

            ConflictException conflictException =>
                ProblemDetails.FromBusinessException(conflictException, instance),

            BusinessRuleException businessRuleException =>
                ProblemDetails.FromBusinessException(businessRuleException, instance),

            ExternalServiceException externalServiceException =>
                ProblemDetails.FromBusinessException(externalServiceException, instance),

            ConcurrencyException concurrencyException =>
                ProblemDetails.FromBusinessException(concurrencyException, instance),

            RateLimitException rateLimitException =>
                ProblemDetails.FromBusinessException(rateLimitException, instance),

            _ => CreateGenericProblemDetails(exception, instance, traceId)
        };
    }

    private ProblemDetails CreateGenericProblemDetails(Exception exception, string instance, string traceId)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var problemDetails = new ProblemDetails
        {
            Type = ProblemDetails.GetProblemTypeUri("INTERNAL_ERROR"),
            Title = ProblemDetails.GetProblemTitle("INTERNAL_ERROR"),
            Status = 500,
            Detail = isDevelopment ? exception.Message : "An internal server error occurred",
            Instance = instance,
            Extensions = new Dictionary<string, object>
            {
                ["errorCode"] = "INTERNAL_ERROR",
                ["traceId"] = traceId,
                ["exceptionType"] = exception.GetType().Name
            }
        };

        // Add stack trace in development
        if (isDevelopment)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
            }
        }

        return problemDetails;
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BusinessException businessException => businessException.StatusCode,
            ValidationException => 400,
            UnauthorizedException => 401,
            ForbiddenException => 403,
            NotFoundException => 404,
            ConflictException => 409,
            ConcurrencyException => 409,
            RateLimitException => 429,
            _ => 500
        };
    }

    private void LogException(Exception exception, HttpContext context, int statusCode)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

        var logContext = new
        {
            RequestMethod = context.Request.Method,
            RequestPath = context.Request.Path,
            RequestQuery = context.Request.QueryString.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            TraceId = context.TraceIdentifier,
            StatusCode = statusCode
        };

        using (_logger.BeginScope(logContext))
        {
            _logger.Log(logLevel, exception,
                "Exception occurred while processing request: {Message}", exception.Message);
        }

        // For 5xx errors, log additional details
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception details: {StackTrace}", exception.StackTrace);
        }
    }
}

/// <summary>
/// Extension methods for global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// Add global exception handler middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

/// <summary>
/// Custom exception filter for MVC controllers
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var problemDetails = CreateProblemDetails(exception, context);
        var statusCode = GetStatusCode(exception);

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;

        // Log the exception
        _logger.LogError(exception, "Exception caught by ApiExceptionFilter: {Message}", exception.Message);
    }

    private ProblemDetails CreateProblemDetails(Exception exception, ExceptionContext context)
    {
        var instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        return exception switch
        {
            BusinessException businessException =>
                ProblemDetails.FromBusinessException(businessException, instance),

            ValidationException validationException =>
                ProblemDetails.FromValidationException(validationException, instance),

            _ => ProblemDetails.FromException(exception, 500, instance)
        };
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BusinessException businessException => businessException.StatusCode,
            ValidationException => 400,
            _ => 500
        };
    }
}

/// <summary>
/// Extension methods for API exception handling
/// </summary>
public static class ApiExceptionHandlingExtensions
{
    /// <summary>
    /// Add API exception handling services
    /// </summary>
    public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilter>();
        });

        return services;
    }
}
