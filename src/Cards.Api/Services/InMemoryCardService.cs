using Cards.Api.Models;
using Cards.Api.Options;
using Microsoft.Extensions.Options;

namespace Cards.Api.Services;

public class InMemoryCardService(IOptions<CardsOptions> options) : ICardService
{
    private readonly int _maxSize = options.Value.MaxCollectionSize;
    private int _nextId = 4;
    
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

    public Card? Create(CreateCardRequest request)
    {
        lock (_lock)
        {
            if (_cards.Count >= _maxSize) return null;
            
            var card = new Card(_nextId++, request.Name, request.Type, request.Attack);
            _cards.Add(card);
            return card;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _cards.RemoveAll(c => c.Id == id) > 0;
    }
}