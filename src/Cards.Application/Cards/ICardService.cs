using Cards.Domain;
using Cards.Domain.Models;

namespace Cards.Application.Cards;

public interface ICardService
{
    Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct);
    Task<Card?> GetByIdAsync(int id, CancellationToken ct);
    Task<Card> CreateAsync(CreateCardCommand command, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}