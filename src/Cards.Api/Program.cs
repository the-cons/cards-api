var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/ping", () => new { status = "pong", time = DateTime.UtcNow });
app.MapControllers();

app.Run();
