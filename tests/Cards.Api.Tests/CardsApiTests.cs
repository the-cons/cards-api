using System.Net;
using System.Net.Http.Json;
using Cards.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Tests;

public class CardsApiTests : IDisposable
{
    private readonly CardsApiFactory _factory = new();
    private readonly HttpClient _client;
    
    public CardsApiTests() => _client = _factory.CreateClient();
    
    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_returns_seeded_cards()
    {
        var cards = await _client.GetFromJsonAsync<List<Card>>("/api/cards");
        
        Assert.NotNull(cards);
        Assert.Equal(3, cards.Count);
        Assert.Contains(cards, card => card.Name == "Dark Magician");
    }
    
    [Fact]
    public async Task GetById_unknown_returns_404()
    {
        var response = await _client.GetAsync("/api/cards/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_valid_card_returns_201_and_is_retrievable()
    {
        var request = new CreateCardRequest("Red-Eyes Black Dragon", "Dragon", 2400);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await _client.GetFromJsonAsync<Card>(response.Headers.Location);
        Assert.Equal("Red-Eyes Black Dragon", created?.Name);
    }

    [Fact]
    public async Task Create_when_collection_is_full_returns_409()
    {
        var existing = (await _client.GetFromJsonAsync<List<Card>>("/api/cards"))!.Count;
        for (var i = existing; i < 5; i++)
        {
            var fill = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest($"Filler {i}", "Normal", 100));
            Assert.Equal(HttpStatusCode.Created, fill.StatusCode);
        }
        
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest($"One too many", "Normal", 100));
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Collection is full", problem?.Title);
    }

    [Fact]
    public async Task Create_when_name_present_returns_409()
    {
        var responseA = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest($"Some name once", "Normal", 100));
        Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);
        
        var responseB = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest($"Some name once", "Normal", 100));
        Assert.Equal(HttpStatusCode.Conflict, responseB.StatusCode);
        var problem = await responseB.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Card already exists", problem?.Title);
    }
    
    [Theory]
    [InlineData("", "Dragon", 1000, "Name")]
    [InlineData("Valid", "Dragon", 9000, "Attack")]
    [InlineData("Valid", "Dragon", null, "Attack")]
    public async Task Create_invalid_card_returns_400(string name, string type, int? attack, string field)
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest(name, type, attack));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Fact]
    public async Task Delete_existing_returns_204_then_404()
    {
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/api/cards/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,  (await _client.DeleteAsync("/api/cards/1")).StatusCode);
    }
}