using Cards.Api.Data;
using Cards.Application;
using Cards.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Cards.Api.Services;

public class EfCardRepository(CardsDbContext db) : ICardRepository
{
    public async Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct) =>
        await db.Cards
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new Card(c.Id, c.Name, c.Type, c.Attack))
            .ToListAsync(ct);

    public Task<Card?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Cards
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new Card(c.Id, c.Name, c.Type, c.Attack))
            .FirstOrDefaultAsync(ct);

    public Task<int> CountAsync(CancellationToken ct) => db.Cards.CountAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct) =>
        db.Cards.AnyAsync(c => c.Name == name, ct);

    public async Task<Card> AddAsync(string name, string type, int attack, CancellationToken ct)
    {
        var entity = new CardEntity { Name = name, Type = type, Attack = attack };
        db.Cards.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            throw new DuplicateCardException(name); // translate the DB detail into a domain error
        }

        return new Card(entity.Id, entity.Name, entity.Type, entity.Attack);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await db.Cards.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
}