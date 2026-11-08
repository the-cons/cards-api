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

    [HttpPost]
    public ActionResult<Card> Create(CreateCardRequest request)
    {
        var card = cards.Create(request);
        if (card is null)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Collection is full");
        return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
    }

    [HttpDelete(("{id:int}"))]
    public ActionResult Delete(int id) => cards.Delete(id) ? NoContent() : NotFound();
}