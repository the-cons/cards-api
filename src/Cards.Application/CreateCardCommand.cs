namespace Cards.Application;

public record CreateCardCommand(string Name, string Type, int Attack);