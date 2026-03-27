using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia_poker_game.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public MainWindowViewModel()
    {
        CurrentPage = CreateMenu();
    }
    
    private MenuViewModel CreateMenu()
    {
        return new MenuViewModel(
            NavigateToGame, 
            NavigateToSettings
        );
    }
    
    private void NavigateToGame()
    {
        CurrentPage = new GameViewModel(
            onBack: NavigateToMenu
        );
    }
    
    private void NavigateToMenu()
    {
        CurrentPage = CreateMenu();
    }
    
    private void NavigateToSettings()
    {
        CurrentPage = new SettingsViewModel(
            NavigateToMenu
        );
    }
}