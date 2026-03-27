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
    public partial CardBackOption SelectedCardBack { get; set; } =
        CardBackOptions.FirstOrDefault(o => o.Name == AppSettings.Instance.CardBackColor)
        ?? CardBackOptions[0];

    // Auto-save card back as soon as the user clicks a new one
    partial void OnSelectedCardBackChanged(CardBackOption value)
    {
        if (value is not null)
            AppSettings.Instance.CardBackColor = value.Name;
    }

    [ObservableProperty]
    public partial bool SoundEffects { get; set; } = true;

    [ObservableProperty]
    public partial bool BackgroundMusic { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowAnimations { get; set; } = true;

    [ObservableProperty]
    public partial bool AutoFold { get; set; } = false;

    [ObservableProperty]
    public partial bool ShowHandHints { get; set; } = false;

    [ObservableProperty]
    public partial bool AllowChatDuringPlay { get; set; } = true;
    
    // Go back 
    [RelayCommand]
    private void Back() => _onBack();
}
