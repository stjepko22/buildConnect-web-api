using System.Reflection;
using BuildConnect.Common;
using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class ApplicationRuntimeInfoProvider
{
    public ApiMetadata GetMetadata()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(ApplicationRuntimeInfoProvider).Assembly;
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        return new ApiMetadata(BuildConnectDefaults.ApplicationName, environment, version);
    }
}
