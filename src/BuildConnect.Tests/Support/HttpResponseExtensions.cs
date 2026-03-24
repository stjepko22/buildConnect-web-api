using System.Net.Http.Json;

namespace BuildConnect.Tests.Support;

internal static class HttpResponseExtensions
{
    public static async Task<T> ReadRequiredJsonAsync<T>(this HttpResponseMessage response)
    {
        var model = await response.Content.ReadFromJsonAsync<T>();
        return model ?? throw new InvalidOperationException($"Expected JSON body of type {typeof(T).Name}.");
    }
}
