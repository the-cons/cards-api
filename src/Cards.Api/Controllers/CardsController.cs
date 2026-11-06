using Cards.Api.Models;
using Cards.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public class CardsController(ICardService cards) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<Card> GetAll() => cards.GetAll();

    [HttpGet("{id:int}")]
    public ActionResult<Card> GetById(int id)
    {
        var card = cards.GetById(id);
        return card is null ? NotFound() : card;
    }
}