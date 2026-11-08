namespace Cards.Api.Domain;

public class DuplicateCardException(string name)
    : Exception($"The card already exists with the name {name}")
{
    public string Name { get; } = name;
}