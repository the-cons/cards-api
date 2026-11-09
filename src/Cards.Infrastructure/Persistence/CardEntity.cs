namespace Cards.Infrastructure.Persistence;

internal class CardEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public int Attack  { get; set; }
}