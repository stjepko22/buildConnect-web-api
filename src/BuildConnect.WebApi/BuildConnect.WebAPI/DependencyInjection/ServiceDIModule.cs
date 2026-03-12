using BuildConnect.Service;
using BuildConnect.Service.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class ServiceDIModule
{
    public static IServiceCollection AddServiceModule(this IServiceCollection services)
    {
        services.AddScoped<IApiStatusService, ApiStatusService>();
        services.AddScoped<IJobService, JobService>();

        return services;
    }
}
