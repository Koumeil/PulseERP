using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Abstractions.Settings;

namespace PulseERP.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure JWT authentication avec challenge WWW-Authenticate sur 401.
    /// </summary>
    public static IServiceCollection AddJwtWithChallenge(
        this IServiceCollection services,
        JwtSettings jwtSettings
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // paramètres classiques
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),
                    ClockSkew = TimeSpan.Zero,
                };

                // challenge 401 enrichi
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.Headers.Append(
                            "WWW-Authenticate",
                            $"Bearer realm=\"PulseERP\", error=\"invalid_token\", error_description=\"{ctx.ErrorDescription ?? "Unauthorized"}\""
                        );
                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
