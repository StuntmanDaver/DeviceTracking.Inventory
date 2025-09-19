using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeviceTracking.Inventory.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request
        LogRequest(context);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            LogResponse(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private void LogRequest(HttpContext context)
    {
        var request = context.Request;

        var logContext = new
        {
            RequestId = context.TraceIdentifier,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.Headers["User-Agent"].ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            User = context.User?.Identity?.Name ?? "Anonymous"
        };

        _logger.LogInformation("HTTP Request: {Method} {Path}{QueryString} from {RemoteIpAddress}",
            request.Method, request.Path, request.QueryString, context.Connection.RemoteIpAddress);
    }

    private void LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        var response = context.Response;

        var logContext = new
        {
            RequestId = context.TraceIdentifier,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            ElapsedMilliseconds = elapsedMilliseconds
        };

        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "HTTP Response: {StatusCode} in {ElapsedMilliseconds}ms for request {RequestId}",
            response.StatusCode, elapsedMilliseconds, context.TraceIdentifier);
    }
}

/// <summary>
/// Extension methods for request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Add request logging middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
