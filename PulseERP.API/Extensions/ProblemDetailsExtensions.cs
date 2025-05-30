using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Settings;

namespace PulseERP.API.Extensions;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Configure RFC7807 ProblemDetails avec mappings métier et extensions.
    /// </summary>
    public static IServiceCollection AddCustomProblemDetails(
        this IServiceCollection services,
        JwtSettings jwtSettings,
        IHostEnvironment env
    )
    {
        services.AddProblemDetails(opts =>
        {
            // Toujours masquer stack trace / exceptionDetails
            opts.IncludeExceptionDetails = (_, __) => false;

            // Validation → 400
            opts.Map<ValidationException>(
                (ctx, ex) =>
                {
                    var pd = new ValidationProblemDetails(ex.Errors)
                    {
                        Type = "https://docs.pulserp.com/errors#validation_failed",
                        Title = "Validation failed",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Voir le champ 'errors' pour plus de détails.",
                        Instance = ctx.Request.Path,
                    };
                    return pd;
                }
            );

            // NotFound → 404
            opts.Map<NotFoundException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#not_found",
                        Title = "Resource not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = ex.Message,
                        Instance = ctx.Request.Path,
                    }
            );

            // Unauthorized → 401
            opts.Map<UnauthorizedAccessException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#unauthorized",
                        Title = "Unauthorized",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = ex.Message,
                        Instance = ctx.Request.Path,
                    }
            );

            // Fallback → 500
            opts.Map<Exception>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#internal_server_error",
                        Title = "Internal server error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = env.IsDevelopment()
                            ? ex.Message
                            : "Une erreur inattendue est survenue.",
                        Instance = ctx.Request.Path,
                    }
            );

            // Ajout d'extensions communes
            opts.OnBeforeWriteDetails = (ctx, pd) =>
            {
                pd.Extensions["traceId"] = ctx.TraceIdentifier;
                pd.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");
                pd.Extensions["errorCode"] = pd.Status switch
                {
                    StatusCodes.Status400BadRequest => "validation_failed",
                    StatusCodes.Status401Unauthorized => "auth:unauthorized",
                    StatusCodes.Status404NotFound => "not_found",
                    StatusCodes.Status500InternalServerError => "internal_server_error",
                    _ => "error",
                };
            };
        });

        return services;
    }
}
