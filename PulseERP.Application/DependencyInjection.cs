using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Services;

namespace PulseERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBrandService, BrandService>();

        // AutoMapper configuration
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
