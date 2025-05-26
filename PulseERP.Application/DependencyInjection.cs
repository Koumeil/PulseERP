using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Services;
using PulseERP.Contracts.Interfaces.Services;

namespace PulseERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
