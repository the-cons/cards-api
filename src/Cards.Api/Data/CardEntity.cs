namespace Cards.Api.Data;

public class CardEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public int Attack  { get; set; }
}