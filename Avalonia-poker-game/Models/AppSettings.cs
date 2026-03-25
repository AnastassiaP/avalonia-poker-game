namespace Avalonia_poker_game.Models;

/// <summary>
/// Settings that will be shared in the app
/// </summary>
public static class AppSettings
{
    public static string CardBackColor { get; set; } = "blue";

    public static string CardBackPath =>
        $"avares://Avalonia-poker-game/Assets/Cards/back-{CardBackColor}.png";
}
