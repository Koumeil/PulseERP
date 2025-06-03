using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Settings;
using PulseERP.Domain.Errors;

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

            opts.Map<ValidationException>(
                (ctx, ex) =>
                {
                    // LOG ici !
                    var logger = ctx.RequestServices.GetRequiredService<
                        ILogger<ValidationException>
                    >();
                    logger.LogWarning("ValidationException: {Errors}", ex.Errors);

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

            // DomainException → 400 (business rule violation)
            opts.Map<DomainException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#domain_error",
                        Title = "Business rule violation",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message,
                        Instance = ctx.Request.Path,
                    }
            );

            // ConflictException → 409
            opts.Map<ConflictException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#conflict",
                        Title = "Conflict",
                        Status = StatusCodes.Status409Conflict,
                        Detail = ex.Message,
                        Instance = ctx.Request.Path,
                    }
            );

            // UnauthorizedException → 401
            opts.Map<UnauthorizedException>(
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

            // ForbiddenException → 403
            opts.Map<ForbiddenException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#forbidden",
                        Title = "Forbidden",
                        Status = StatusCodes.Status403Forbidden,
                        Detail = ex.Message,
                        Instance = ctx.Request.Path,
                    }
            );

            // BadRequestException → 400
            opts.Map<BadRequestException>(
                (ctx, ex) =>
                    new ProblemDetails
                    {
                        Type = "https://docs.pulserp.com/errors#bad_request",
                        Title = "Bad request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message,
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
                    StatusCodes.Status409Conflict => "conflict",
                    StatusCodes.Status500InternalServerError => "internal_server_error",
                    _ => "error",
                };

                // Cherche un correlationId dans les headers (par ex "X-Request-Id")
                if (ctx.Request.Headers.TryGetValue("X-Request-Id", out var correlationId))
                    pd.Extensions["correlationId"] = correlationId.ToString();
            };
        });

        return services;
    }
}
