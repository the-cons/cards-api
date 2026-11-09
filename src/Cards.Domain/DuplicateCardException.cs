namespace Cards.Domain;

public class DuplicateCardException(string name)
    : DomainException($"The card already exists with the name {name}")
{
    public string Name { get; } = name;
}