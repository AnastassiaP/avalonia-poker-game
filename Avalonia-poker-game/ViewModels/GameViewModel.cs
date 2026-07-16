using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia_poker_game.Models;

namespace Avalonia_poker_game.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private const int SmallBlind = 10;
    private const int BigBlind = 20;

    private readonly Deck _deck = new();
    private readonly Random _rng = new();
    
    // Play order
    private static readonly int[] SeatOrder = [0, 1, 2, 3, 4];

    private int _dealerIndex = -1;
    private int _currentPlayerIndex;
    private int _playersToAct;
    private int _tableBet;

    public ObservableCollection<Player> Players { get; } = [];
    public ObservableCollection<Card> CommunityCards { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PotText))]
    public partial int Pot { get; set; }

    [ObservableProperty]
    public partial string CurrentPhase { get; set; } = "Pre-Flop";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Welcome to Poker!";

    [ObservableProperty]
    public partial bool ShowNewHandButton { get; set; }

    [ObservableProperty]
    public partial int RaiseAmount { get; set; } = 40;

    [ObservableProperty]
    public partial int MaxRaise { get; set; } = 1000;

    [ObservableProperty]
    public partial int MinRaise { get; set; } = 20;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FoldCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckCallCommand))]
    [NotifyCanExecuteChangedFor(nameof(RaiseCommand))]
    public partial bool IsPlayerTurn { get; set; }

    public Player Human => Players[0];
    public Player Ai1   => Players[1];
    public Player Ai2   => Players[2];
    public Player Ai3   => Players[3];
    public Player Ai4   => Players[4];
    
    public string CardBackPath => AppSettings.Instance.CardBackPath;

    public string PotText  => $"Pot: ${Pot}";
    public string CallText => _tableBet > Human.CurrentBet
        ? $"Call  ${_tableBet - Human.CurrentBet}"
        : "Check";
    

    private readonly Action? _onBack;

    public GameViewModel(Action? onBack = null)
    {
        _onBack = onBack;
        Players.Add(new Player("You",      isHuman: true,  ""));
        Players.Add(new Player("Tigeress",  isHuman: false, "avares://Avalonia-poker-game/Assets/Characters/tigeress.png"));
        Players.Add(new Player("Mr. Pig",  isHuman: false, "avares://Avalonia-poker-game/Assets/Characters/mr_pig.png"));
        Players.Add(new Player("Capitan",  isHuman: false, "avares://Avalonia-poker-game/Assets/Characters/capitan_shark.png"));
        Players.Add(new Player("Ratto",    isHuman: false, "avares://Avalonia-poker-game/Assets/Characters/the_rat.png"));

        _ = StartNewHandAsync();
    }

    [RelayCommand(CanExecute = nameof(IsPlayerTurn))]
    private async Task Fold()
    {
        IsPlayerTurn = false;
        Human.IsFolded = true;
        Human.IsTurn   = false;
        StatusMessage = "You folded.";
        await AfterPlayerActed();
    }

    [RelayCommand(CanExecute = nameof(IsPlayerTurn))]
    private async Task CheckCall()
    {
        IsPlayerTurn = false;
        int toCall = _tableBet - Human.CurrentBet;
        if (toCall > 0)
        {
            int actual = Math.Min(toCall, Human.Chips);
            Human.Chips      -= actual;
            Human.CurrentBet += actual;
            Pot              += actual;
            StatusMessage = $"You called ${actual}.";
        }
        else
        {
            StatusMessage = "You checked.";
        }
        Human.IsTurn = false;
        await AfterPlayerActed();
    }

    [RelayCommand(CanExecute = nameof(IsPlayerTurn))]
    private async Task Raise()
    {
        IsPlayerTurn = false;
        int actual = Math.Min(RaiseAmount, Human.Chips);
        Human.Chips      -= actual;
        Human.CurrentBet += actual;
        Pot              += actual;

        if (Human.CurrentBet > _tableBet)
        {
            _tableBet = Human.CurrentBet;
            // everyone else needs to act again
            _playersToAct = ActivePlayers().Count - 1;
        }

        StatusMessage = $"You raised to ${Human.CurrentBet}.";
        OnPropertyChanged(nameof(CallText));
        Human.IsTurn = false;
        await AfterPlayerActed();
    }

    [RelayCommand]
    private async Task NewHand() => await StartNewHandAsync();

    [RelayCommand]
    private void BackToMenu() => _onBack?.Invoke();

    private async Task StartNewHandAsync()
    {
        ShowNewHandButton = false;
        CommunityCards.Clear();
        Pot = 0;
        _tableBet = 0;
        CurrentPhase = "Pre-Flop";

        foreach (var p in Players)
        {
            p.Hand.Clear();
            p.CurrentBet  = 0;
            p.IsFolded    = false;
            p.IsTurn      = false;
            p.ShowCards   = false;
            if (p.Chips == 0) p.IsEliminated = true;
        }

        var active = ActivePlayers();
        if (active.Count < 2) { StatusMessage = "Game over!"; return; }

        _deck.Reset();
        _deck.Shuffle();
        foreach (var p in active) { p.Hand.Add(_deck.Deal()); p.Hand.Add(_deck.Deal()); }
        
        _dealerIndex = NextActiveIndex(_dealerIndex);

        int sbIdx = NextActiveIndex(_dealerIndex);
        int bbIdx = NextActiveIndex(sbIdx);

        PostBlind(Players[sbIdx], SmallBlind);
        PostBlind(Players[bbIdx], BigBlind);
        _tableBet = BigBlind;

        // pre-flop: UTG is after BB
        _currentPlayerIndex = NextActiveIndex(bbIdx);
        _playersToAct       = active.Count;

        StatusMessage = "Cards dealt. Good luck!";
        await ActivateCurrentPlayer();
    }

    private async Task ActivateCurrentPlayer()
    {
        int guard = 0;
        while ((Players[_currentPlayerIndex].IsFolded || Players[_currentPlayerIndex].IsEliminated)
               && guard++ < Players.Count)
            _currentPlayerIndex = NextSeatIndex(_currentPlayerIndex);

        var current = Players[_currentPlayerIndex];
        current.IsTurn = true;

        if (current.IsHuman)
        {
            MaxRaise  = current.Chips;
            MinRaise  = Math.Max(BigBlind, _tableBet - current.CurrentBet + BigBlind);
            RaiseAmount = Math.Clamp(RaiseAmount, MinRaise, MaxRaise);
            OnPropertyChanged(nameof(CallText));
            IsPlayerTurn  = true;
            StatusMessage = "Your turn!";
            // TODO: Execution pauses here — the player's button click drives the next step
        }
        else
        {
            StatusMessage = $"{current.Name} is thinking…";
            await Task.Delay(1200);
            await ExecuteAiAction(current);
        }
    }

    private async Task ExecuteAiAction(Player ai)
    {
        int toCall = _tableBet - ai.CurrentBet;

        if (toCall > 0 && _rng.NextDouble() < 0.30)
        {
            ai.IsFolded = true;
            StatusMessage = $"{ai.Name} folds.";
        }
        else if (toCall > 0)
        {
            int actual = Math.Min(toCall, ai.Chips);
            ai.Chips      -= actual;
            ai.CurrentBet += actual;
            Pot           += actual;
            StatusMessage = $"{ai.Name} calls ${actual}.";
        }
        else
        {
            StatusMessage = $"{ai.Name} checks.";
        }

        ai.IsTurn = false;
        await AfterPlayerActed();
    }

    private async Task AfterPlayerActed()
    {
        _playersToAct--;

        var active = ActivePlayers();
        if (active.Count <= 1)
        {
            AwardPot(active.Count == 1 ? active[0] : Players[0]);
            return;
        }

        if (_playersToAct <= 0)
        {
            await AdvancePhase();
            return;
        }

        _currentPlayerIndex = NextSeatIndex(_currentPlayerIndex);
        await ActivateCurrentPlayer();
    }

    private async Task AdvancePhase()
    {
        foreach (var p in Players) p.CurrentBet = 0;
        _tableBet = 0;
        OnPropertyChanged(nameof(CallText));

        var active = ActivePlayers();
        if (active.Count <= 1) { AwardPot(active.Count == 1 ? active[0] : Players[0]); return; }

        switch (CurrentPhase)
        {
            case "Pre-Flop":
                CurrentPhase = "Flop";
                CommunityCards.Add(_deck.Deal());
                CommunityCards.Add(_deck.Deal());
                CommunityCards.Add(_deck.Deal());
                StatusMessage = "The Flop!";
                break;
            case "Flop":
                CurrentPhase = "Turn";
                CommunityCards.Add(_deck.Deal());
                StatusMessage = "The Turn!";
                break;
            case "Turn":
                CurrentPhase = "River";
                CommunityCards.Add(_deck.Deal());
                StatusMessage = "The River!";
                break;
            case "River":
                CurrentPhase = "Showdown";
                DoShowdown();
                return;
        }
        
        _currentPlayerIndex = NextActiveIndex(_dealerIndex);
        _playersToAct       = active.Count;
        await ActivateCurrentPlayer();
    }

    private void DoShowdown()
    {
        // Reveal all active AI hands
        foreach (var p in ActivePlayers())
            if (!p.IsHuman) p.ShowCards = true;

        var active = ActivePlayers();
        var results = active.ToDictionary(
            p => p,
            p => HandEvaluator.EvaluateBest([.. p.Hand, .. CommunityCards]));

        var best = results.Values.Max()!;
        var winners = results.Where(kv => kv.Value.CompareTo(best) == 0)
                              .Select(kv => kv.Key)
                              .ToList();

        // Kickers only matter to explain the win when someone else at the end
        // had the same combination and the win/tie came down to the kicker cards.
        var showKickers = results.Values.Count(r => r.Combination == best.Combination) > 1;

        AwardShowdownPot(winners, best, showKickers);
    }

    private void AwardShowdownPot(List<Player> winners, HandResult winningHand, bool showKickers)
    {
        // Only attribute the win to a specific player's hole cards when there's a single
        // winner — with a split pot, the tied winners' hole cards can differ, so there's
        // no one hand to point to.
        var holeRanks = winners.Count == 1
            ? new HashSet<Rank>(winners[0].Hand.Select(c => c.Rank))
            : null;
        var description = HandEvaluator.Describe(winningHand, showKickers, holeRanks);
        var share     = Pot / winners.Count;
        var remainder = Pot % winners.Count;

        foreach (var w in winners) w.Chips += share;
        if (remainder > 0) winners[0].Chips += remainder;

        StatusMessage = winners.Count == 1
            ? $"{(winners[0].IsHuman ? "You" : winners[0].Name)} win{(winners[0].IsHuman ? "" : "s")} ${Pot} — {description}!"
            : $"{string.Join(", ", winners.Select(w => w.IsHuman ? "You" : w.Name))} split ${Pot} — {description}!";

        Pot                = 0;
        IsPlayerTurn       = false;
        ShowNewHandButton  = true;
    }

    private void AwardPot(Player winner)
    {
        winner.Chips += Pot;
        StatusMessage = winner.IsHuman ? $"You win ${Pot}!" : $"{winner.Name} wins ${Pot}!";
        Pot           = 0;
        IsPlayerTurn  = false;
        ShowNewHandButton = true;
    }

    private List<Player> ActivePlayers() =>
        [.. Players.Where(p => !p.IsFolded && !p.IsEliminated)];

    private int NextActiveIndex(int from)
    {
        int idx = NextSeatIndex(from);
        int guard = 0;
        while (Players[idx].IsEliminated && guard++ < Players.Count)
            idx = NextSeatIndex(idx);
        return idx;
    }

    private int NextSeatIndex(int from)
    {
        var pos = Array.IndexOf(SeatOrder, from);
        return SeatOrder[(pos + 1) % SeatOrder.Length];
    }

    private void PostBlind(Player player, int amount)
    {
        int actual = Math.Min(amount, player.Chips);
        player.Chips      -= actual;
        player.CurrentBet  = actual;
        Pot               += actual;
    }
}
