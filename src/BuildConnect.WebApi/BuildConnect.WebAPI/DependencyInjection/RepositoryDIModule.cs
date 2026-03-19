using BuildConnect.Repository;
using BuildConnect.Repository.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class RepositoryDIModule
{
    public static IServiceCollection AddRepositoryModule(this IServiceCollection services)
    {
        services.AddScoped<IApiStatusRepository, ApiStatusRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
