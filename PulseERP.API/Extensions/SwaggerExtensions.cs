using Microsoft.OpenApi.Models;

namespace PulseERP.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // Swagger document
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PulseERP API", Version = "v1" });

            // Utilisation de noms complets pour éviter les collisions de schémas
            c.CustomSchemaIds(type => GenerateUniqueSchemaId(type));

            static string GenerateUniqueSchemaId(Type type)
            {
                if (type.IsGenericType)
                {
                    var genericTypeName = type.GetGenericTypeDefinition().Name.Split('`')[0];
                    var genericArgs = string.Join(
                        "_",
                        type.GetGenericArguments().Select(GenerateUniqueSchemaId)
                    );
                    return $"{genericTypeName}_{genericArgs}";
                }

                return type.FullName!.Replace(".", "_").Replace("+", "_");
            }

            // Définition JWT
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
}
