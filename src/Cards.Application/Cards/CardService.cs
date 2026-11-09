using System.Diagnostics;
using Cards.Application.Diagnostics;
using Cards.Application.Ports;
using Cards.Domain;
using Cards.Domain.Exceptions;
using Cards.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cards.Application.Cards;

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
            throw Reject(activity, command.Name, "full", new CollectionFullException(max));

        if (await repository.ExistsByNameAsync(command.Name, ct))
            throw Reject(activity, command.Name, "duplicate", new DuplicateCardException(command.Name));

        Card card;
        try
        {
            card = await repository.AddAsync(command.Name, command.Type, command.Attack, ct);
        }
        catch (DuplicateCardException ex)   // lost the race: the repository translated the DB error
        {
            Reject(activity, command.Name, "duplicate", ex);
            throw;
        }

        LogCardCreated(logger, card.Id, card.Name);
        CardsTelemetry.CardsCreated.Add(1, new KeyValuePair<string, object?>("card.type", card.Type));
        activity?.SetTag("card.id", card.Id);
        return card;
    } 
    
    private DomainException Reject(Activity? activity, string cardName, string reason, DomainException exception)
    {
        LogCardRejected(logger, cardName, reason, exception.Message);
        activity?.SetStatus(ActivityStatusCode.Error, reason);
        CardsTelemetry.CardsRejected.Add(1, new KeyValuePair<string, object?>("reason", reason));
        return exception;
    }
}