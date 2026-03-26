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
    private int _chips = 1000;
    
    [ObservableProperty] 
    private int _currentBet;
    
    [ObservableProperty] 
    private bool _isTurn;
    
    [ObservableProperty] 
    private bool _isFolded;
    
    [ObservableProperty] 
    private bool _isEliminated;
}
