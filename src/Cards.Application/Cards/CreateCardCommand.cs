namespace Cards.Application.Cards;

public record CreateCardCommand(string Name, string Type, int Attack);