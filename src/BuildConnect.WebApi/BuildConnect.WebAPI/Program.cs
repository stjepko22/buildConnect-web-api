using BuildConnect.DAL;
using BuildConnect.Repository;
using BuildConnect.Repository.Common;
using BuildConnect.Service;
using BuildConnect.Service.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ApplicationRuntimeInfoProvider>();
builder.Services.AddScoped<IApiStatusRepository, ApiStatusRepository>();
builder.Services.AddScoped<IApiStatusService, ApiStatusService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    message = "BuildConnect API is running.",
    statusEndpoint = "/api/health"
}));

app.Run();

public partial class Program
{
}
