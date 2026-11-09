namespace Cards.Domain;

public class CollectionFullException(int maxSize)
    : DomainException($"The collection is full (max {maxSize} cards).")
{
    public int MaxSize { get; } = maxSize;
}