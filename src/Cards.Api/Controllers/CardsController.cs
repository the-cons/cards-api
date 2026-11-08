using Cards.Api.Models;
using Cards.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public class CardsController(ICardService cards) : ControllerBase
{
    /// <summary>
    /// Lists all cards in the collection.
    /// </summary>
    [HttpGet]
    public async Task<IReadOnlyList<Card>> GetAll(CancellationToken ct) => 
        await cards.GetAllAsync(ct);

    /// <summary>
    /// Returns a card by the given card ID from the collection.
    /// </summary>
    /// <remarks>ID needs to be in the collection. Fails with 404 if not found.</remarks>
    /// <param name="id">The ID of the requested card.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType<Card>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Card>> GetById(int id, CancellationToken ct)
    {
        var card = await cards.GetByIdAsync(id, ct);
        return card is null ? NotFound() : card;
    }

    /// <summary>Adds a card to the collection.</summary>
    /// <remarks>Card names must be unique. Fails with 409 when the collection is full.</remarks>
    /// <param name="request">The card to create.</param>
    [HttpPost]
    [ProducesResponseType<Card>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Card>> Create(CreateCardRequest request,  CancellationToken ct)
    {
        var card = await cards.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
    }

    /// <summary>
    /// Deletes a card from the Collection by the given ID.
    /// </summary>
    /// <remarks>ID needs to be in the collection. Fails with 404 if not found.</remarks>
    /// <param name="id">The ID of the requested card.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct) => 
        await cards.DeleteAsync(id, ct) ? NoContent() : NotFound();
}