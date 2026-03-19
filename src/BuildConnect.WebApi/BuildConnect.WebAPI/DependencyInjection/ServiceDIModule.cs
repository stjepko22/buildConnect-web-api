using BuildConnect.Service;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class ServiceDIModule
{
    public static IServiceCollection AddServiceModule(this IServiceCollection services)
    {
        services.AddScoped<IApiStatusService, ApiStatusService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
