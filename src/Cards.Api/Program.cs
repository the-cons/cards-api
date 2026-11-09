using Cards.Api.Setup;
using Cards.Application;
using Cards.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddCardsTelemetry();
builder.Services
    .AddCardsApplication()
    .AddCardsInfrastructure(builder.Configuration)
    .AddCardsAuth()
    .AddCardsWeb();

var app = builder.Build();

app.UseCardsPipeline();
await app.MigrateCardsDatabaseIfEnabledAsync();
app.MapCardsEndpoints();

app.Run();