using System.ComponentModel.DataAnnotations;

namespace Cards.Application.Cards;

public class CardsOptions
{
    public const string Section = "Cards";

    [Range(1, 10_000)] public int MaxCollectionSize { get; set; } = 100;
}