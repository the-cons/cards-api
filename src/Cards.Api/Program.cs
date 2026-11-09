using System.Security.Claims;
using Cards.Api.Data;
using Cards.Api.Errors;
using Cards.Api.Services;
using Cards.Application;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("cards-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource(CardsTelemetry.Name))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddMeter(CardsTelemetry.Name))
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;
    o.IncludeFormattedMessage = true;
});

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
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICardRepository, EfCardRepository>();
builder.Services.AddOptions<CardsOptions>()
    .BindConfiguration(CardsOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath
                                                      | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
    o.CombineLogs = true;   // one log line per request instead of separate request and response lines
});

var app = builder.Build();

app.UseHttpLogging();
app.UseExceptionHandler();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    using (logger.BeginScope(new Dictionary<string, object>
           {
               ["User"] = context.User.Identity?.Name ?? "anonymous"
           }))
    {
        await next(context);   // ≈ chain.doFilter(request, response)
    }
});

app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CardsDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
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