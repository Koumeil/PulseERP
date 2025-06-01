using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Abstractions.Security.Interfaces;
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
        services.AddAutoMapper(
            (sp, cfg) =>
            {
                cfg.AddProfile(new UserProfile());
                cfg.AddProfile(new PhoneNumberProfile());
                cfg.AddProfile(new CustomerProfile());
                cfg.AddProfile(new ProductProfile());
                cfg.AddProfile(new BrandProfile());
                cfg.AddProfile(new EmailProfile());
                cfg.AddProfile(new AddressProfile());
                cfg.AddProfile(new AuthProfile());
            },
            // On passe un tableau vide d'Assembly pour désactiver le scan automatique
            Array.Empty<Assembly>()
        );

        return services;
    }
}
