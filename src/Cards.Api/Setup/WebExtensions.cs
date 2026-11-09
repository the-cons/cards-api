using Cards.Api.Errors;
using Microsoft.AspNetCore.HttpLogging;

namespace Cards.Api.Setup;

public static class WebExtensions
{
    public static IServiceCollection AddCardsWeb(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddHttpLogging(o =>
        {
            o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath
                                                              | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
            o.CombineLogs = true;
        });
        return services;
    }
}