using Cards.Application.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cards.Api.Setup;

public static class TelemetryExtensions
{
    public static IHostApplicationBuilder AddCardsTelemetry(this IHostApplicationBuilder builder)
    {
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

        return builder;
    } 
}