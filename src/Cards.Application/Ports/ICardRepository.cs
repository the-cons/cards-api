using Cards.Domain;
using Cards.Domain.Exceptions;
using Cards.Domain.Models;

namespace Cards.Application.Ports;

/// <summary>
/// Persistence port.
/// </summary>
public interface ICardRepository
{
    Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct);
    Task<Card?> GetByIdAsync(int id, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);

    /// <exception cref="DuplicateCardException">The name is already taken (e.g. a concurrent insert won).</exception>
    Task<Card> AddAsync(string name, string type, int attack, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}