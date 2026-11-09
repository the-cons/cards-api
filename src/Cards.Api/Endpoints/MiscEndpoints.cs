using System.Security.Claims;

namespace Cards.Api.Endpoints;

public static class MiscEndpoints
{
    public static IEndpointRouteBuilder MapMiscEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping", () => new { status = "pong", time = DateTime.UtcNow })
            .WithName("Ping")
            .WithSummary("Liveness check")
            .WithTags("Health");

        var auth = routes.MapGroup("/api").WithTags("Auth").RequireAuthorization();
        auth.MapGet("/me", GetMe);

        return routes;
    }

    private static object GetMe(ClaimsPrincipal user) => new
    {
        name = user.Identity?.Name,
        isAdmin = user.IsInRole("admin"),
        claims = user.Claims.Select(c => new { c.Type, c.Value })
    };
}