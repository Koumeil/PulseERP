using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Services;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Identity;
using PulseERP.Infrastructure.Logging;
using PulseERP.Infrastructure.Repositories;

namespace PulseERP.Infrastructure;

public static class DependencyInjection
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
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

        // Repositories
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();

        // Identity services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        return services;
    }
}
