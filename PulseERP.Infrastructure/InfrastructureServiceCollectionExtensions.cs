using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Identity;
using PulseERP.Infrastructure.Logging;
using PulseERP.Infrastructure.Persistence;
using PulseERP.Infrastructure.Provider;
using PulseERP.Infrastructure.Repositories;

namespace PulseERP.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Logging
        services.AddScoped(typeof(ISerilogAppLoggerService<>), typeof(SerilogAppLoggerService<>));

        // Database context
        services.AddDbContext<CoreDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        // DateTime provider
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();

        // Identity services
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<ITokenRepository, TokenRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
        services.AddScoped<ITokenHasherService, TokenHasherService>();
        return services;
    }
}
