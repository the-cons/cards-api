using System.ComponentModel.DataAnnotations;

namespace Cards.Api.Options;

public class CardsOptions
{
    public const string Section = "Cards";
    
    [Range(1, 10_000)]
    public int MaxCollectionSize { get; set; } = 100;
}