using BuildConnect.WebAPI.DependencyInjection;

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

        app.MapControllers();
        app.MapGet("/", () => Results.Ok(new
        {
            message = "BuildConnect API is running.",
            statusEndpoint = "/api/health"
        }));
    }
}
