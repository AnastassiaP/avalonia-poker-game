namespace Avalonia_poker_game.Models;

public static class HandEvaluator
{
    public static HandResult EvaluateBest(IReadOnlyList<Card> cards)
    {
        if (cards.Count < 5)
            throw new ArgumentException("At least five cards are required.", nameof(cards));

        HandResult? bestHand = null;

        foreach (var fiveCards in ChooseFive(cards))
        {
            var currentHand = EvaluateFive(fiveCards);

            if (bestHand is null || currentHand.CompareTo(bestHand) > 0)
                bestHand = currentHand;
        }

        return bestHand!;
    }

    public static string Describe(HandResult hand, bool includeKickers = true, IReadOnlySet<Rank>? holeRanks = null)
    {
        var tieBreakers = hand.TieBreakers;

        // Explains why the player the others
        string KickerText(IEnumerable<Rank> ranks, string label)
        {
            var list = holeRanks is null ? ranks.ToList() : ranks.Where(holeRanks.Contains).ToList();
            return list.Count > 0 ? $" ({label}: {string.Join(", ", list)})" : "";
        }

        return hand.Combination switch
        {
            Combination.RoyalFlush =>
                "Royal Flush",

            Combination.StraightFlush =>
                includeKickers ? $"Straight Flush, {tieBreakers[0]} high" : "Straight Flush",

            Combination.FourOfAKind =>
                $"Four of a Kind, {tieBreakers[0]}s" +
                (includeKickers ? KickerText(tieBreakers.Skip(1), "your kicker") : ""),

            Combination.FullHouse =>
                $"Full House, {tieBreakers[0]}s full of {tieBreakers[1]}s",

            Combination.Flush =>
                includeKickers ? $"Flush, {string.Join("-", tieBreakers)} high" : $"Flush, {tieBreakers[0]} high",

            Combination.Straight =>
                includeKickers ? $"Straight, {tieBreakers[0]} high" : "Straight",

            Combination.ThreeOfAKind =>
                $"Three of a Kind, {tieBreakers[0]}s" +
                (includeKickers ? KickerText(tieBreakers.Skip(1), "your kickers") : ""),

            Combination.TwoPair =>
                $"Two Pair, {tieBreakers[0]}s and {tieBreakers[1]}s" +
                (includeKickers ? KickerText(tieBreakers.Skip(2), "your kicker") : ""),

            Combination.Pair =>
                $"Pair of {tieBreakers[0]}s" +
                (includeKickers ? KickerText(tieBreakers.Skip(1), "your kickers") : ""),

            Combination.HighCard =>
                includeKickers ? $"High Card, {string.Join("-", tieBreakers)} high" : $"High Card, {tieBreakers[0]} high",

            _ =>
                throw new ArgumentOutOfRangeException()
        };
    }

    private static IEnumerable<List<Card>> ChooseFive(
        IReadOnlyList<Card> cards)
    {
        var cardCount = cards.Count;

        for (var first = 0; first < cardCount; first++)
        for (var second = first + 1; second < cardCount; second++)
        for (var third = second + 1; third < cardCount; third++)
        for (var fourth = third + 1; fourth < cardCount; fourth++)
        for (var fifth = fourth + 1; fifth < cardCount; fifth++)
        {
            yield return
            [
                cards[first],
                cards[second],
                cards[third],
                cards[fourth],
                cards[fifth]
            ];
        }
    }

    private static HandResult EvaluateFive(List<Card> cards)
    {
        var rankGroups = cards
            .GroupBy(card => card.Rank)
            .Select(group => new
            {
                Rank = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(group => group.Count)
            .ThenByDescending(group => group.Rank)
            .ToList();

        var ranksDescending = cards
            .Select(card => card.Rank)
            .OrderByDescending(rank => rank)
            .ToList();

        var isFlush = cards.All(card => card.Suit == cards[0].Suit);

        var straightHigh = GetStraightHigh(ranksDescending.Distinct());
        var isStraight = straightHigh.HasValue;

        if (isFlush && isStraight)
        {
            var combination = straightHigh == Rank.Ace
                ? Combination.RoyalFlush
                : Combination.StraightFlush;

            return CreateResult(
                combination,
                [straightHigh.Value],
                cards);
        }

        if (rankGroups[0].Count == 4)
        {
            return CreateResult(
                Combination.FourOfAKind,
                [rankGroups[0].Rank, rankGroups[1].Rank],
                cards);
        }

        if (rankGroups[0].Count == 3 &&
            rankGroups[1].Count == 2)
        {
            return CreateResult(
                Combination.FullHouse,
                [rankGroups[0].Rank, rankGroups[1].Rank],
                cards);
        }

        if (isFlush)
        {
            return CreateResult(
                Combination.Flush,
                ranksDescending,
                cards);
        }

        if (isStraight)
        {
            return CreateResult(
                Combination.Straight,
                [straightHigh.Value],
                cards);
        }

        if (rankGroups[0].Count == 3)
        {
            var kickers = rankGroups
                .Skip(1)
                .Select(group => group.Rank);

            return CreateResult(
                Combination.ThreeOfAKind,
                [rankGroups[0].Rank, .. kickers],
                cards);
        }

        if (rankGroups[0].Count == 2 &&
            rankGroups[1].Count == 2)
        {
            return CreateResult(
                Combination.TwoPair,
                [
                    rankGroups[0].Rank,
                    rankGroups[1].Rank,
                    rankGroups[2].Rank
                ],
                cards);
        }

        if (rankGroups[0].Count == 2)
        {
            var kickers = rankGroups
                .Skip(1)
                .Select(group => group.Rank);

            return CreateResult(
                Combination.Pair,
                [rankGroups[0].Rank, .. kickers],
                cards);
        }

        return CreateResult(
            Combination.HighCard,
            ranksDescending,
            cards);
    }

    private static HandResult CreateResult(
        Combination combination,
        IEnumerable<Rank> tieBreakers,
        List<Card> cards)
    {
        return new HandResult(
            combination,
            tieBreakers.ToList(),
            cards);
    }

    private static Rank? GetStraightHigh(
        IEnumerable<Rank> distinctRanks)
    {
        var sortedRanks = distinctRanks
            .Distinct()
            .OrderByDescending(rank => rank)
            .ToList();

        if (sortedRanks.Count < 5)
            return null;

        for (var index = 0;
             index <= sortedRanks.Count - 5;
             index++)
        {
            var highest = (int)sortedRanks[index];
            var lowest = (int)sortedRanks[index + 4];

            if (highest - lowest == 4)
                return sortedRanks[index];
        }

        var hasAceLowStraight =
            sortedRanks.Contains(Rank.Ace) &&
            sortedRanks.Contains(Rank.Two) &&
            sortedRanks.Contains(Rank.Three) &&
            sortedRanks.Contains(Rank.Four) &&
            sortedRanks.Contains(Rank.Five);

        return hasAceLowStraight
            ? Rank.Five
            : null;
    }
}