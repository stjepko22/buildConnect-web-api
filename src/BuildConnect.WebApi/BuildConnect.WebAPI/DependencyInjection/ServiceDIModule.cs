using BuildConnect.Service;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using BuildConnect.WebAPI.Email;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.WebAPI.DependencyInjection;

public static class ServiceDIModule
{
    public static IServiceCollection AddServiceModule(this IServiceCollection services)
    {
        services.AddScoped<IApiStatusService, ApiStatusService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<IEmailVerificationSender, ResendEmailVerificationSender>((serviceProvider, httpClient) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            httpClient.BaseAddress = new Uri(configuration["Resend:BaseUrl"] ?? "https://api.resend.com/");
        });
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
