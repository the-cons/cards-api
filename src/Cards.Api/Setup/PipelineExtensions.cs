using Cards.Api.Endpoints;
using Cards.Infrastructure;
using Scalar.AspNetCore;

namespace Cards.Api.Setup;

public static class PipelineExtensions
{
    public static WebApplication UseCardsPipeline(this WebApplication app)
    {
        // ORDER MATTERS: each middleware wraps everything below it
        app.UseHttpLogging();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseMiddleware<UserLogScopeMiddleware>();   // needs the authenticated user
        app.UseAuthorization();
        return app;
    }

    public static async Task MigrateCardsDatabaseIfEnabledAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
            await app.Services.MigrateCardsDatabaseAsync();
    }

    public static WebApplication MapCardsEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapMiscEndpoints();
        app.MapControllers();
        return app;
    }
}