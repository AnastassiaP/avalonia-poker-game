namespace Avalonia_poker_game.Models;

public class HandResult : IComparable<HandResult>
{
    public Combination Combination { get; }
    public List<Rank> TieBreakers { get; }
    public List<Card> BestCards { get; }

    public HandResult(Combination combination, List<Rank> tieBreakers, List<Card> bestCards)
    {
        Combination = combination;
        TieBreakers = tieBreakers;
        BestCards = bestCards;
    }

    public int CompareTo(HandResult? other)
    {
        if (other is null) return 1;

        var combinationCompare = Combination.CompareTo(other.Combination);
        if (combinationCompare != 0)
            return combinationCompare;

        for (var i = 0; i < TieBreakers.Count && i < other.TieBreakers.Count; i++)
        {
            var rankCompare = TieBreakers[i].CompareTo(other.TieBreakers[i]);
            if (rankCompare != 0)
                return rankCompare;
        }

        return 0;
    }
}
