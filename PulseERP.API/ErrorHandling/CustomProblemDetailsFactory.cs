using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace PulseERP.API.ErrorHandling;

/// <summary>
/// Factory personnalisée pour créer des ProblemDetails enrichis,
/// avec trace d'identifiant de requête unique et logging.
/// </summary>
public class CustomProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly ApiBehaviorOptions _apiBehaviorOptions;
    private readonly ILogger<CustomProblemDetailsFactory> _logger;

    public CustomProblemDetailsFactory(
        IOptions<ApiBehaviorOptions> apiBehaviorOptions,
        ILogger<CustomProblemDetailsFactory> logger
    )
    {
        _apiBehaviorOptions =
            apiBehaviorOptions?.Value
            ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        statusCode ??= StatusCodes.Status500InternalServerError;

        var problemDetails = new ExtendedProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance ?? GetRequestPath(httpContext),
            TimestampUtc = DateTime.UtcNow,
            RequestId = GetRequestId(httpContext),
            ErrorCode = GetDefaultErrorCode(statusCode.Value),
        };

        ApplyDefaults(httpContext, problemDetails, statusCode.Value);
        LogProblem(problemDetails);

        return problemDetails;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelState,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        statusCode ??= StatusCodes.Status400BadRequest;

        var validationDetails = new ExtendedValidationProblemDetails(modelState)
        {
            Status = statusCode,
            Title = title ?? "Validation error",
            Type = type,
            Detail = detail,
            Instance = instance ?? GetRequestPath(httpContext),
            TimestampUtc = DateTime.UtcNow,
            RequestId = GetRequestId(httpContext),
            ErrorCode = "validation:invalid_request",
        };

        ApplyDefaults(httpContext, validationDetails, statusCode.Value);
        LogProblem(validationDetails);

        return validationDetails;
    }

    #region Helpers

    private void ApplyDefaults(HttpContext context, ProblemDetails details, int statusCode)
    {
        if (_apiBehaviorOptions.ClientErrorMapping.TryGetValue(statusCode, out var clientError))
        {
            details.Title ??= clientError.Title;
            details.Type ??= clientError.Link;
        }

        details.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
        details.Extensions["machine"] = Environment.MachineName;

        if (context.Items.TryGetValue("ExtraErrorInfo", out var extraData))
        {
            details.Extensions["extraInfo"] = extraData;
        }

        if (statusCode == StatusCodes.Status500InternalServerError && !IsDevelopment())
        {
            details.Detail =
                "An unexpected error occurred. Please contact support with the request ID.";
        }
    }

    private void LogProblem(ProblemDetails details)
    {
        var extendedDetails = details as ExtendedProblemDetails;

        if (details.Status >= 500)
        {
            _logger.LogError(
                "Server error {Status} [{ErrorCode}] - {Title} (RequestId: {RequestId})",
                details.Status,
                extendedDetails?.ErrorCode ?? "unknown_error",
                details.Title ?? "No Title",
                extendedDetails?.RequestId ?? "No RequestId"
            );
        }
        else if (details.Status >= 400)
        {
            _logger.LogWarning(
                "Client error {Status} [{ErrorCode}] - {Title}",
                details.Status,
                extendedDetails?.ErrorCode ?? "unknown_warning",
                details.Title ?? "No Title"
            );
        }
    }

    private static string GetRequestPath(HttpContext context) =>
        context.Request.Path.HasValue ? context.Request.Path.Value! : "/";

    private static string GetRequestId(HttpContext context) => context.TraceIdentifier;

    private static string GetDefaultErrorCode(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "bad_request",
            StatusCodes.Status401Unauthorized => "unauthorized",
            StatusCodes.Status403Forbidden => "forbidden",
            StatusCodes.Status404NotFound => "not_found",
            StatusCodes.Status500InternalServerError => "internal_server_error",
            _ => $"http_{statusCode}",
        };

    private static bool IsDevelopment() =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    #endregion
}
