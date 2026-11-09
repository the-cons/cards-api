using Cards.Domain;

namespace Cards.Application;

public interface ICardService
{
    Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct);
    Task<Card?> GetByIdAsync(int id, CancellationToken ct);
    Task<Card> CreateAsync(CreateCardCommand command, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}