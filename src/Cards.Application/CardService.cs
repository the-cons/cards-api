using System.Diagnostics;
using Cards.Domain;
using Microsoft.Extensions.Logging;     // ← must be explicit here, see below
using Microsoft.Extensions.Options;

namespace Cards.Application;

public partial class CardService(
    ICardRepository repository,
    IOptions<CardsOptions> options,
    ILogger<CardService> logger) : ICardService
{
    public Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct) => repository.GetAllAsync(ct);
    public Task<Card?> GetByIdAsync(int id, CancellationToken ct) => repository.GetByIdAsync(id, ct);
    public Task<bool> DeleteAsync(int id, CancellationToken ct) => repository.DeleteAsync(id, ct);

    public async Task<Card> CreateAsync(CreateCardCommand command, CancellationToken ct)
    {
        using var activity = CardsTelemetry.ActivitySource.StartActivity("CreateCard");
        activity?.SetTag("card.name", command.Name);

        var max = options.Value.MaxCollectionSize;
        if (await repository.CountAsync(ct) >= max)
        {
            LogCardTooMany(logger, command.Name, max);
            activity?.SetStatus(ActivityStatusCode.Error, "collection full");
            CardsTelemetry.CardsRejected.Add(1, new KeyValuePair<string, object?>("reason", "full"));
            throw new CollectionFullException(max);
        }

        if (await repository.ExistsByNameAsync(command.Name, ct))
        {
            LogCardDuplicate(logger, command.Name);
            activity?.SetStatus(ActivityStatusCode.Error, "card already exists");
            CardsTelemetry.CardsRejected.Add(1, new KeyValuePair<string, object?>("reason", "duplicate"));
            throw new DuplicateCardException(command.Name);
        }

        Card card;
        try
        {
            card = await repository.AddAsync(command.Name, command.Type, command.Attack, ct);
        }
        catch (DuplicateCardException)   // lost the race: the repository translated the DB error for us
        {
            LogCardDuplicate(logger, command.Name);
            activity?.SetStatus(ActivityStatusCode.Error, "card already exists");
            CardsTelemetry.CardsRejected.Add(1, new KeyValuePair<string, object?>("reason", "duplicate"));
            throw;
        }

        LogCardCreated(logger, card.Id, card.Name);
        CardsTelemetry.CardsCreated.Add(1, new KeyValuePair<string, object?>("card.type", card.Type));
        activity?.SetTag("card.id", card.Id);
        return card;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created card {CardId} named {CardName}")]
    private static partial void LogCardCreated(ILogger logger, int cardId, string cardName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Card {CardName} already exists")]
    private static partial void LogCardDuplicate(ILogger logger, string cardName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Collection full, rejected {CardName} (max {MaxSize})")]
    private static partial void LogCardTooMany(ILogger logger, string cardName, int maxSize);
}