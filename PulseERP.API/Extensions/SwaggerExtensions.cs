using Microsoft.OpenApi.Models;

namespace PulseERP.API.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Configure Swagger/OpenAPI with JWT Bearer support and utilise FullName (namespace + nom de type) pour les schemaIds.
    /// </summary>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // Déclarez votre document Swagger (v1 ici)
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PulseERP API", Version = "v1" });

            // Utiliser FullName pour désambiguïser les noms de schéma
            c.CustomSchemaIds(type => type.FullName!);

            // Configuration JWT Bearer
            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Entrez 'Bearer {token}'",
                }
            );

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });

        return services;
    }

    /// <summary>
    /// Registers Swagger middleware in the request pipeline.
    /// </summary>
    public static WebApplication UseCustomSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PulseERP API V1");
        });

        return app;
    }
}
