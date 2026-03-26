namespace Avalonia_poker_game.Models;

public class Card
{
    public Suit Suit { get; }
    public Rank Rank { get; }
    public string ImagePath => $"avares://Avalonia-poker-game/Assets/Cards/{SuitCode}_{RankCode}.png";

    private string SuitCode => Suit switch
    {
        Suit.Clubs    => "club",
        Suit.Diamonds => "diamond",
        Suit.Hearts   => "heart",
        Suit.Spades   => "spade",
        _ => ""
    };

    private string RankCode => Rank switch
    {
        Rank.Ace   => "1",
        Rank.Jack  => "jack",
        Rank.Queen => "queen",
        Rank.King  => "king",
        _ => ((int)Rank).ToString()
    };

    public Card(Suit suit, Rank rank)
    {
        Suit = suit; 
        Rank = rank;
    }
    public override string ToString() => $"{RankCode}{SuitCode}";
}
