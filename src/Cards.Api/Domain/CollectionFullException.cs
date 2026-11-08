namespace Cards.Api.Domain;

public class CollectionFullException(int maxSize)
: Exception($"The collection is full (max {maxSize} cards).")
{
    public int MaxSize { get; } =  maxSize;
}