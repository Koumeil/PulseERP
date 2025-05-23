using System;
using Microsoft.Extensions.DependencyInjection;
using PulseERP.Application.Common.Interfaces;
using PulseERP.Infrastructure.Logging;

namespace PulseERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        return services;
    }
}