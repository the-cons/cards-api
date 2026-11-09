using Cards.Api.Data;
using Cards.Api.Domain;
using Cards.Api.Models;
using Cards.Api.Options;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cards.Api.Services;

public partial class EfCardService(
    CardsDbContext db, 
    IOptions<CardsOptions> options,
    ILogger<EfCardService> logger) : ICardService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Created card {CardId} named {CardName}")]
    private static partial void LogCardCreated(ILogger logger, int cardId, string cardName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Card {CardName} already exists")]
    private static partial void LogCardDuplicate(ILogger logger, string cardName);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "Collection full, rejected {CardName} (max {MaxSize})")]
    private static partial void LogCardTooMany(ILogger logger, string cardName, int maxSize);
    
    public async Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct) =>
        await db.Cards
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new Card(c.Id, c.Name, c.Type, c.Attack))
            .ToListAsync(ct);

    public async Task<Card?> GetByIdAsync(int id, CancellationToken ct) =>
        await db.Cards
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new Card(c.Id, c.Name, c.Type, c.Attack))
            .FirstOrDefaultAsync(ct);

    public async Task<Card> CreateAsync(CreateCardRequest request, CancellationToken ct)
    {
        var max = options.Value.MaxCollectionSize;
        if (await db.Cards.CountAsync(ct) >= max)
        {
            LogCardTooMany(logger, request.Name, max);
            throw new CollectionFullException(max);
        }

        if (await db.Cards.AnyAsync(c => c.Name == request.Name, ct))
        {
            LogCardDuplicate(logger, request.Name);
            throw new DuplicateCardException(request.Name);
        }

        var entity = new CardEntity
        {
            Name = request.Name,
            Type = request.Type,
            Attack = request.Attack!.Value
        };

        db.Cards.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            LogCardCreated(logger, entity.Id, entity.Name);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            LogCardDuplicate(logger, entity.Name);
            throw new DuplicateCardException(request.Name);
        }

        return new Card(entity.Id, entity.Name, entity.Type, entity.Attack);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await db.Cards.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
}