using Cards.Api.Models;

namespace Cards.Api.Services;

public interface ICardService
{
    Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct);
    Task<Card?> GetByIdAsync(int id, CancellationToken ct);
    Task<Card?> CreateAsync(CreateCardRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}