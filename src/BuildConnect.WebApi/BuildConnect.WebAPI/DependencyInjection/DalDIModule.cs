using BuildConnect.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class DalDIModule
{
    public static IServiceCollection AddDalModule(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationRuntimeInfoProvider>();
        services.AddSingleton<InMemoryAuthDataStore>();
        services.AddSingleton<InMemoryBidDataStore>();
        services.AddSingleton<InMemoryJobDataStore>();
        services.AddSingleton<InMemoryReviewDataStore>();
        services.AddSingleton<InMemoryUserDataStore>();

        return services;
    }
}
