using Cards.Api.Data;
using Cards.Api.Domain;
using Cards.Api.Models;
using Cards.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cards.Api.Services;

public class EfCardService(CardsDbContext db, IOptions<CardsOptions> options) : ICardService
{
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
            throw new CollectionFullException(max);

        if (await db.Cards.AnyAsync(c => c.Name == request.Name, ct))
            throw new DuplicateCardException(request.Name);

        var entity = new CardEntity
        {
            Name = request.Name,
            Type = request.Type,
            Attack = request.Attack!.Value
        };

        db.Cards.Add(entity);
        await db.SaveChangesAsync(ct);

        return new Card(entity.Id, entity.Name, entity.Type, entity.Attack);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await db.Cards.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
}