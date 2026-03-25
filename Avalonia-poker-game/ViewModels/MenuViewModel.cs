using System;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia_poker_game.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly Action? _onPlay;
    private readonly Action? _onSettings;

    public MenuViewModel(Action? onPlay = null, Action? onSettings = null)
    {
        _onPlay = onPlay;
        _onSettings = onSettings;
    }

    [RelayCommand]
    private void Play() => _onPlay?.Invoke();

    [RelayCommand]
    private void Settings() => _onSettings?.Invoke();
}