using Cards.Api.Models;

namespace Cards.Api.Services;

public interface ICardService
{
    IReadOnlyList<Card> GetAll();
    Card? GetById(int id);
}