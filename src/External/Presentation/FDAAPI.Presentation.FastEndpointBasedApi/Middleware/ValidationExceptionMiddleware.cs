using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Middleware;

/// <summary>
/// Middleware to catch FluentValidation.ValidationException thrown from MediatR pipeline
/// and convert it to a proper 400 Bad Request response.
/// </summary>
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for request {Path}", context.Request.Path);
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            success = false,
            message = "Validation failed",
            errors = exception.Errors.Select(error => new
            {
                field = error.PropertyName,
                message = error.ErrorMessage,
                attemptedValue = error.AttemptedValue,
                severity = error.Severity.ToString()
            }).ToList(),
            statusCode = 400
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
