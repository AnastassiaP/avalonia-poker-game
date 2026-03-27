namespace Avalonia_poker_game.Models;


public interface IAppSettings
{
    string CardBackColor { get; set; }
    string CardBackPath { get; }
}

public sealed class AppSettings : IAppSettings
{
    public static AppSettings Instance { get; } = new AppSettings();

    private AppSettings() { }

    public string CardBackColor { get; set; } = "blue";

    public string CardBackPath =>
        $"avares://Avalonia-poker-game/Assets/Cards/back-{CardBackColor}.png";
}
