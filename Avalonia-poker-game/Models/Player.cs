using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Avalonia_poker_game.Models;

public partial class Player : ObservableObject
{
    public Player(string name, bool isHuman, string characterImagePath = "")
    {
        Name = name;
        IsHuman = isHuman;
        CharacterImagePath = characterImagePath;
    }

    public string Name { get; }
    public bool IsHuman { get; }
    public string CharacterImagePath { get; }
    public ObservableCollection<Card> Hand { get; } = [];

    [ObservableProperty]
    public partial int Chips { get; set; } = 1000;

    [ObservableProperty]
    public partial int CurrentBet { get; set; }

    [ObservableProperty]
    public partial bool IsTurn { get; set; }

    [ObservableProperty]
    public partial bool IsFolded { get; set; }

    [ObservableProperty]
    public partial bool IsEliminated { get; set; }

    [ObservableProperty]
    public partial bool ShowCards { get; set; }
}
