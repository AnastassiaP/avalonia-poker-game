using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia_poker_game.Models;

namespace Avalonia_poker_game.ViewModels;

public record CardBackOption(string Name, string DisplayName)
{
    public string ImagePath => $"avares://Avalonia-poker-game/Assets/Cards/back-{Name}.png";
}

public partial class SettingsViewModel : ViewModelBase
{
    private readonly Action _onBack;

    public SettingsViewModel(Action onBack) { _onBack = onBack; }
    
    public SettingsViewModel() : this(() => { }) { }

    public static IReadOnlyList<CardBackOption> CardBackOptions { get; } =
    [
        new("blue",    "Blue"),
        new("red",     "Red"),
        new("green",   "Green"),
        new("black",   "Black"),
        new("purple",  "Purple"),
        new("navy",    "Navy"),
        new("teal",    "Teal"),
        new("aqua",    "Aqua"),
        new("fuchsia", "Fuchsia"),
        new("gray",    "Gray"),
        new("silver",  "Silver"),
        new("lime",    "Lime"),
        new("maroon",  "Maroon"),
        new("olive",   "Olive"),
        new("yellow",  "Yellow"),
        new("0062ff",  "Sapphire"),
    ];

    [ObservableProperty]
    private CardBackOption _selectedCardBack =
        CardBackOptions.FirstOrDefault(o => o.Name == AppSettings.CardBackColor)
        ?? CardBackOptions[0];

    // Auto-save card back as soon as the user clicks a new one
    partial void OnSelectedCardBackChanged(CardBackOption value)
    {
        if (value is not null)
            AppSettings.CardBackColor = value.Name;
    }
    
    [ObservableProperty] 
    private bool _soundEffects  = true;
    
    [ObservableProperty] 
    private bool _backgroundMusic = true;
    
    [ObservableProperty] 
    private bool _showAnimations = true;
    
    [ObservableProperty] 
    private bool _autoFold = false;
    
    [ObservableProperty] 
    private bool _showHandHints = false;
    
    [ObservableProperty] 
    private bool _allowChatDuringPlay = true;
    
    // Go back 
    [RelayCommand]
    private void Back() => _onBack();
}
