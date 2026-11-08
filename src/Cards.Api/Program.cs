using Cards.Api.Options;
using Cards.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ICardService, InMemoryCardService>();
builder.Services.AddOptions<CardsOptions>()
    .BindConfiguration(CardsOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.MapGet("/ping", () => new { status = "pong", time = DateTime.UtcNow });
app.MapControllers();

app.Run();
