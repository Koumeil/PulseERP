using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PulseERP.Application.Exceptions;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ProblemDetailsFactory problemDetailsFactory,
        ILogger<GlobalExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _problemDetailsFactory = problemDetailsFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            await HandleExceptionAsync(
                context,
                vex,
                StatusCodes.Status400BadRequest,
                "Validation Error"
            );
        }
        catch (NotFoundException nfex)
        {
            await HandleExceptionAsync(
                context,
                nfex,
                StatusCodes.Status404NotFound,
                "Resource Not Found"
            );
        }
        catch (UnauthorizedAccessException uex)
        {
            await HandleExceptionAsync(
                context,
                uex,
                StatusCodes.Status401Unauthorized,
                "Unauthorized"
            );
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error"
            );
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        int statusCode,
        string title
    )
    {
        _logger.LogError(ex, "Exception caught in middleware");

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: statusCode,
            title: title,
            detail: ex.Message
        );

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
