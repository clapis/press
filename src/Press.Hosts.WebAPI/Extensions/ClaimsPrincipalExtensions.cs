using System.Security.Claims;

namespace Press.Hosts.WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
        => principal.GetRequired(ClaimTypes.NameIdentifier);

    private static string GetRequired(this ClaimsPrincipal principal, string claimType)
        => principal.FindFirstValue(claimType) ?? throw new Exception($"Principal doesn't have required claim '{claimType}'");
}