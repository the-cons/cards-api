using System.Net;
using System.Net.Http.Json;
using Cards.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Tests;

public class CardsApiTests : IDisposable
{
    private readonly CardsApiFactory _factory = new();
    private readonly HttpClient _anonymous;
    private readonly HttpClient _user;
    private readonly HttpClient _admin;
    
    private record MeResponse(string Name, bool IsAdmin);
    
    public CardsApiTests()
    {
        _anonymous= _factory.CreateClient();
        _user = _factory.CreateClientAs("alice");
        _admin = _factory.CreateClientAs("bob", "admin");
    }

    public void Dispose()
    {
        _anonymous.Dispose();
        _user.Dispose();
        _admin.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_returns_seeded_cards()
    {
        var cards = await _anonymous.GetFromJsonAsync<List<Card>>("/api/cards");
        
        Assert.NotNull(cards);
        Assert.Equal(3, cards.Count);
        Assert.Contains(cards, card => card.Name == "Dark Magician");
    }
    
    [Fact]
    public async Task GetById_unknown_returns_404()
    {
        var response = await _anonymous.GetAsync("/api/cards/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_valid_card_returns_201_and_is_retrievable()
    {
        var request = new CreateCardRequest("Red-Eyes Black Dragon", "Dragon", 2400);

        var response = await _user.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await _user.GetFromJsonAsync<Card>(response.Headers.Location);
        Assert.Equal("Red-Eyes Black Dragon", created?.Name);
    }

    [Fact]
    public async Task Create_when_collection_is_full_returns_409()
    {
        var existing = (await _user.GetFromJsonAsync<List<Card>>("/api/cards"))!.Count;
        for (var i = existing; i < 5; i++)
        {
            var fill = await _user.PostAsJsonAsync("/api/cards", new CreateCardRequest($"Filler {i}", "Normal", 100));
            Assert.Equal(HttpStatusCode.Created, fill.StatusCode);
        }
        
        var response = await _user.PostAsJsonAsync("/api/cards", new CreateCardRequest("One too many", "Normal", 100));
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Collection is full", problem?.Title);
    }

    [Fact]
    public async Task Create_when_name_present_returns_409()
    {
        var responseA = await _user.PostAsJsonAsync("/api/cards", new CreateCardRequest("Some name once", "Normal", 100));
        Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);
        
        var responseB = await _user.PostAsJsonAsync("/api/cards", new CreateCardRequest("Some name once", "Normal", 100));
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
        var response = await _user.PostAsJsonAsync("/api/cards", new CreateCardRequest(name, type, attack));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Fact]
    public async Task Delete_existing_returns_204_then_404()
    {
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.DeleteAsync("/api/cards/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,  (await _admin.DeleteAsync("/api/cards/1")).StatusCode);
    }

    [Fact]
    public async Task Create_when_anon_then_401()
    {
        var response = await _anonymous.PostAsJsonAsync("/api/cards", new CreateCardRequest("Some name once", "Normal", 100));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_when_user_returns_403()
    {
        var r = await _user.DeleteAsync("/api/cards/1");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _user.GetAsync("/api/cards/1")).StatusCode);
    }
    
    [Fact]
    public async Task Me_as_admin_returns_identity()
    {
        var me = await _admin.GetFromJsonAsync<MeResponse>("/api/me");

        Assert.Equal("bob", me?.Name);
        Assert.True(me?.IsAdmin);
    }
}