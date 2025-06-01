using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Mapping.Addresses;
using PulseERP.Application.Mapping.Auth;
using PulseERP.Application.Mapping.Brands;
using PulseERP.Application.Mapping.Emails;
using PulseERP.Application.Mapping.Phones;
using PulseERP.Application.Mapping.Products;
using PulseERP.Application.Mapping.Users;
using PulseERP.Application.Services;

namespace PulseERP.Application;

public static class ApplicationServiceCollectionExtensions
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

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ProductProfile).Assembly);
            cfg.AddMaps(typeof(AddressProfile).Assembly);
            cfg.AddMaps(typeof(AuthProfile).Assembly);
            cfg.AddMaps(typeof(BrandProfile).Assembly);
            cfg.AddMaps(typeof(CustomerProfile).Assembly);
            cfg.AddMaps(typeof(EmailProfile).Assembly);
            cfg.AddMaps(typeof(PhoneNumberProfile).Assembly);
            cfg.AddMaps(typeof(UserProfile).Assembly);
        });

        return services;
    }
}
