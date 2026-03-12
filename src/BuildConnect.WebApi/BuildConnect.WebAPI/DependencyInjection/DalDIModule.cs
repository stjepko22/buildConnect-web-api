using BuildConnect.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class DalDIModule
{
    public static IServiceCollection AddDalModule(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationRuntimeInfoProvider>();
        services.AddSingleton<InMemoryJobDataStore>();

        return services;
    }
}
