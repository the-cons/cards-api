using Cards.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public class ClassController : ControllerBase
{
    private static readonly List<Card> Cards =
    [
        new(1, "Blue-Eyes White Dragon", "Dragon", 3000),
        new(2, "Dark Magician", "Spellcaster", 2500),
        new(3, "Kuriboh", "Fiend", 300),
    ];
    
    [HttpGet]
    public IEnumerable<Card> GetAll() => Cards;

    [HttpGet("{id:int}")]
    public ActionResult<Card> GetById(int id)
    {
        var card = Cards.FirstOrDefault(c => c.Id == id);
        return card is null ? NotFound() : card;
    }
}