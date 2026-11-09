namespace Cards.Api.Setup;

public static class AuthExtensions
{
    public const string AdminPolicy = "CardsAdmin";

    public static IServiceCollection AddCardsAuth(this IServiceCollection services)
    {
        services.AddAuthentication().AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters.NameClaimType = "unique_name";
            o.TokenValidationParameters.RoleClaimType = "role";
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, p => p.RequireRole("admin"));

        return services;
    }
}