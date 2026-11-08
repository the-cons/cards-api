using Cards.Api.Data;
using Cards.Api.Errors;
using Cards.Api.Options;
using Cards.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CardsDbContext>().Database.Migrate();
}

app.MapGet("/ping", () => new { status = "pong", time = DateTime.UtcNow });
app.MapControllers();

app.Run();
