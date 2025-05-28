using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Application.Services;

namespace PulseERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBrandService, BrandService>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
