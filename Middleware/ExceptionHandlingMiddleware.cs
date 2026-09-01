using System.Net;
using System.Text.Json;
using FluentValidation;
using PosApi.Common;
using PosApi.Exceptions;

namespace PosApi.Middleware;

/// <summary>
/// Catches every unhandled exception thrown further down the pipeline and converts it into a
/// consistent <see cref="ApiResponse{T}"/> JSON payload with the right HTTP status code.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message, appEx.Errors),
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                "One or more validation errors occurred.",
                validationEx.Errors.Select(e => e.ErrorMessage).ToList()),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "You are not authorized to perform this action.",
                (List<string>?)null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while processing your request.",
                (List<string>?)null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception occurred: {Message}", exception.Message);
        }

        var response = ApiResponse<object>.FailResponse(
            message,
            errors ?? (_environment.IsDevelopment() ? new List<string> { exception.ToString() } : null));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)(HttpStatusCode)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
