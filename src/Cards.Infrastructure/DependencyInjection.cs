using Cards.Application.Ports;
using Cards.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cards.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCardsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CardsDbContext>(o =>
            o.UseSqlite(configuration.GetConnectionString("Cards")));
        services.AddScoped<ICardRepository, EfCardRepository>();

        return services;
    }

    public static async Task MigrateCardsDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CardsDbContext>().Database.MigrateAsync(ct);
    }
}