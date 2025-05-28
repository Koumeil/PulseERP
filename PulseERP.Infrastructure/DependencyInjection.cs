using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Identity.Entities;
using PulseERP.Infrastructure.Identity.Service;
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
        services.AddScoped(typeof(ISerilogAppLoggerService<>), typeof(SerilogAppLoggerService<>));
        // EF Core
        services.AddDbContext<CoreDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );
        // Identity
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CoreDbContext>()
            .AddDefaultTokenProviders();

        // JWT Token service
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        return services;
    }
}
