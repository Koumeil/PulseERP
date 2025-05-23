using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Services;

namespace PulseERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
