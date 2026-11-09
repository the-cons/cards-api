using Cards.Application.Cards;
using Microsoft.Extensions.DependencyInjection;

namespace Cards.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCardsApplication(this IServiceCollection services)
    {
        services.AddScoped<ICardService, CardService>();
        services.AddOptions<CardsOptions>()
            .BindConfiguration(CardsOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return services;
    }
}