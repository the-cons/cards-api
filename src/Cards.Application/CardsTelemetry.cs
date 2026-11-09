using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Cards.Application;

public static class CardsTelemetry
{
    public const string Name = "Cards.Api";

    public static readonly ActivitySource ActivitySource = new(Name);
    public static readonly Meter Meter = new(Name);

    public static readonly Counter<long> CardsCreated =
        Meter.CreateCounter<long>("cards.created", unit: "{card}", description: "Cards added to the collection");

    public static readonly Counter<long> CardsRejected =
        Meter.CreateCounter<long>("cards.rejected", unit: "{card}", description: "Cards that could not be added");
}