using System.Security.Claims;
using BuildConnect.Model;

namespace BuildConnect.WebAPI.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static RequestUserContext? ToRequestUserContext(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = user.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        var displayName = user.FindFirstValue(ClaimTypes.Name);
        return new RequestUserContext(userId.Trim(), role.Trim(), string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim());
    }
}
