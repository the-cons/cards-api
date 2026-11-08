using Cards.Api.Models;
using Cards.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public class CardsController(ICardService cards) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Card>> GetAll(CancellationToken ct) => 
        await cards.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Card>> GetById(int id, CancellationToken ct)
    {
        var card = await cards.GetByIdAsync(id, ct);
        return card is null ? NotFound() : card;
    }

    [HttpPost]
    public async Task<ActionResult<Card>> Create(CreateCardRequest request,  CancellationToken ct)
    {
        var card = await cards.CreateAsync(request, ct);
        if (card is null)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Collection is full");
        return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
    }

    [HttpDelete(("{id:int}"))]
    public async Task<ActionResult> Delete(int id, CancellationToken ct) => 
        await cards.DeleteAsync(id, ct) ? NoContent() : NotFound();
}