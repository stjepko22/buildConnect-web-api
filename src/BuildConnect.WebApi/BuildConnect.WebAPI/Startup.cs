using System.Text;
using BuildConnect.WebAPI.DependencyInjection;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BuildConnect.WebAPI;

public sealed class Startup
{
    private const string FrontendCorsPolicyName = "FrontendCorsPolicy";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var startup = new Startup(builder.Configuration);

        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app);

        app.Run();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var jwtOptionsSection = Configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtOptionsSection);
        var jwtOptions = jwtOptionsSection.Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicyName, policyBuilder =>
            {
                policyBuilder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ClockSkew = TimeSpan.Zero
                };
            });
        services.AddAuthorization();

        services.AddDalModule();
        services.AddRepositoryModule();
        services.AddServiceModule();
    }

    public void Configure(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors(FrontendCorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGet("/", () => Results.Ok(new
        {
            message = "BuildConnect API is running.",
            statusEndpoint = "/api/health"
        }));
    }
}
