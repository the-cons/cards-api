using Cards.Api.Models;

namespace Cards.Api.Services;

public class InMemoryCardService : ICardService
{
    private readonly Lock _lock = new();
    private readonly List<Card> _cards =
    [
        new(1, "Blue-Eyes White Dragon", "Dragon", 3000),
        new(2, "Dark Magician", "Spellcaster", 2500),
        new(3, "Kuriboh", "Fiend", 300),
    ];
    
    public IReadOnlyList<Card> GetAll()
    {
        lock (_lock) return _cards.ToList();
    }

    public Card? GetById(int id)
    {
        lock (_lock) return _cards.FirstOrDefault(c => c.Id == id);
    }
}