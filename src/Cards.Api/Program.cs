using System.Security.Claims;
using Cards.Api.Data;
using Cards.Api.Errors;
using Cards.Api.Options;
using Cards.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters.NameClaimType = "unique_name";
    o.TokenValidationParameters.RoleClaimType = "role";
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CardsAdmin", p => p.RequireRole("admin"));

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddDbContext<CardsDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Cards")));

builder.Services.AddControllers();
builder.Services.AddScoped<ICardService, EfCardService>();
builder.Services.AddOptions<CardsOptions>()
    .BindConfiguration(CardsOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CardsDbContext>().Database.Migrate();

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/ping", () => new { status = "pong", time = DateTime.UtcNow })
    .WithName("Ping")
    .WithSummary("Liveness check")
    .WithTags("Health");

app.MapGet("/api/me", (ClaimsPrincipal user) => new
    {
        name = user.Identity?.Name,
        isAdmin = user.IsInRole("admin"),
        claims = user.Claims.Select(c => new { c.Type, c.Value })
    })
    .RequireAuthorization()
    .WithTags("Auth");

app.MapControllers();

app.Run();