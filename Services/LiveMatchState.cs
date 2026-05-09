using CricketScoreboardApp.Models;
using CricketScoreboardApp.Models.Enums;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class LiveMatchState
{
    private readonly IMatchService _matchService;
    private readonly IPlayerService _playerService;

    public Match? CurrentMatch { get; private set; }
    public event Action? OnStateChanged;

    public LiveMatchState(IMatchService matchService, IPlayerService playerService)
    {
        _matchService = matchService;
        _playerService = playerService;
    }

    public void SetMatch(Match match)
    {
        CurrentMatch = match;
        NotifyStateChanged();
    }

    public async Task LoadLiveMatchAsync()
    {
        CurrentMatch = await _matchService.LoadLiveMatchAsync();
    }

    public Innings? GetCurrentInnings()
    {
        if (CurrentMatch == null) return null;
        return CurrentMatch.CurrentInningsNumber == 1
            ? CurrentMatch.FirstInnings
            : CurrentMatch.SecondInnings;
    }

    public async Task RecordBall(int runs, ExtraType extraType = ExtraType.None, int extraRuns = 0,
        bool isWicket = false, WicketType wicketType = WicketType.None, string? dismissedPlayerId = null,
        string? fielderId = null)
    {
        if (CurrentMatch == null) return;
        var innings = GetCurrentInnings();
        if (innings == null) return;

        bool isLegal = extraType != ExtraType.Wide && extraType != ExtraType.NoBall;

        var ball = new BallEvent
        {
            OverNumber = innings.TotalBalls / 6,
            BallNumber = innings.TotalBalls % 6 + 1,
            StrikerId = innings.CurrentStrikerId ?? "",
            NonStrikerId = innings.CurrentNonStrikerId ?? "",
            BowlerId = innings.CurrentBowlerId ?? "",
            RunsScored = runs,
            ExtraRuns = extraType == ExtraType.Wide || extraType == ExtraType.NoBall ? extraRuns + 1 : extraRuns,
            ExtraType = extraType,
            IsWicket = isWicket,
            WicketType = wicketType,
            DismissedPlayerId = dismissedPlayerId,
            FielderId = fielderId,
            IsLegalDelivery = isLegal,
            IsBoundary = runs == 4 && extraType == ExtraType.None,
            IsSix = runs == 6 && extraType == ExtraType.None
        };

        // Handle extras correctly
        if (extraType == ExtraType.Wide)
        {
            ball.RunsScored = 0;
            ball.ExtraRuns = 1 + extraRuns; // 1 for wide + any additional runs
        }
        else if (extraType == ExtraType.NoBall)
        {
            ball.ExtraRuns = 1; // 1 for no ball
            ball.RunsScored = runs; // Runs scored off no ball go to batter
        }
        else if (extraType == ExtraType.Bye || extraType == ExtraType.LegBye)
        {
            ball.ExtraRuns = runs;
            ball.RunsScored = 0;
        }

        // Add ball to innings
        innings.AllBalls.Add(ball);

        // Update current over
        var currentOver = innings.Overs.LastOrDefault();
        if (currentOver == null || currentOver.IsComplete)
        {
            currentOver = new Over
            {
                OverNumber = innings.Overs.Count,
                BowlerId = innings.CurrentBowlerId ?? ""
            };
            innings.Overs.Add(currentOver);
        }
        currentOver.Balls.Add(ball);

        // Update score
        innings.TotalRuns += ball.TotalRuns;
        if (isLegal) innings.TotalBalls++;

        // Update extras
        switch (extraType)
        {
            case ExtraType.Wide:
                innings.Wides += ball.ExtraRuns;
                innings.Extras += ball.ExtraRuns;
                break;
            case ExtraType.NoBall:
                innings.NoBalls += 1;
                innings.Extras += 1;
                break;
            case ExtraType.Bye:
                innings.Byes += ball.ExtraRuns;
                innings.Extras += ball.ExtraRuns;
                break;
            case ExtraType.LegBye:
                innings.LegByes += ball.ExtraRuns;
                innings.Extras += ball.ExtraRuns;
                break;
        }

        // Update batting scorecard
        UpdateBattingScore(innings, ball);

        // Update bowling scorecard
        UpdateBowlingScore(innings, ball);

        // Handle wicket
        if (isWicket)
        {
            innings.TotalWickets++;
            var dismissedId = dismissedPlayerId ?? innings.CurrentStrikerId;
            if (dismissedId != null)
            {
                var battingScore = innings.BattingScorecard.FirstOrDefault(b => b.PlayerId == dismissedId);
                if (battingScore != null)
                {
                    battingScore.IsOut = true;
                    battingScore.WicketType = wicketType;
                    battingScore.BowlerId = innings.CurrentBowlerId;
                    battingScore.FielderId = fielderId;
                    battingScore.IsCurrentBatter = false;
                    battingScore.DismissalText = GetDismissalText(wicketType, innings.CurrentBowlerId, fielderId, innings);
                }

                // Fall of wicket
                innings.FallOfWickets.Add($"{innings.TotalRuns}/{innings.TotalWickets} ({innings.OversDisplay})");
            }
        }

        // Rotate strike on odd runs (for legal deliveries and no-balls with runs)
        bool shouldRotate = false;
        if (extraType == ExtraType.Wide)
        {
            if (extraRuns % 2 != 0) shouldRotate = true;
        }
        else
        {
            int totalBatterRuns = runs;
            if (extraType == ExtraType.Bye || extraType == ExtraType.LegBye)
                totalBatterRuns = ball.ExtraRuns;
            if (totalBatterRuns % 2 != 0) shouldRotate = true;
        }

        if (shouldRotate && !isWicket)
        {
            RotateStrike(innings);
        }

        // Check over complete
        if (isLegal && currentOver.LegalBalls >= 6)
        {
            currentOver.IsComplete = true;
            // Rotate strike at end of over
            if (!isWicket) RotateStrike(innings);

            // Update maiden
            var bowlingScore = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == currentOver.BowlerId);
            if (bowlingScore != null && currentOver.IsMaiden)
                bowlingScore.Maidens++;
        }

        // Save state
        await SaveState();
        NotifyStateChanged();
    }

    public async Task UndoLastBall()
    {
        if (CurrentMatch == null) return;
        var innings = GetCurrentInnings();
        if (innings == null || innings.AllBalls.Count == 0) return;

        var lastBall = innings.AllBalls.Last();

        // Check if we need to undo over completion and strike rotation
        var currentOver = innings.Overs.LastOrDefault();
        bool wasOverComplete = currentOver?.IsComplete ?? false;

        // Reverse score
        innings.TotalRuns -= lastBall.TotalRuns;
        if (lastBall.IsLegalDelivery) innings.TotalBalls--;

        // Reverse extras
        switch (lastBall.ExtraType)
        {
            case ExtraType.Wide:
                innings.Wides -= lastBall.ExtraRuns;
                innings.Extras -= lastBall.ExtraRuns;
                break;
            case ExtraType.NoBall:
                innings.NoBalls -= 1;
                innings.Extras -= 1;
                break;
            case ExtraType.Bye:
                innings.Byes -= lastBall.ExtraRuns;
                innings.Extras -= lastBall.ExtraRuns;
                break;
            case ExtraType.LegBye:
                innings.LegByes -= lastBall.ExtraRuns;
                innings.Extras -= lastBall.ExtraRuns;
                break;
        }

        // Reverse batting score
        var batterScore = innings.BattingScorecard.FirstOrDefault(b => b.PlayerId == lastBall.StrikerId);
        if (batterScore != null && lastBall.ExtraType != ExtraType.Bye && lastBall.ExtraType != ExtraType.LegBye
            && lastBall.ExtraType != ExtraType.Wide)
        {
            batterScore.RunsScored -= lastBall.RunsScored;
            if (lastBall.IsLegalDelivery || lastBall.ExtraType == ExtraType.NoBall)
                batterScore.BallsFaced--;
            if (lastBall.IsBoundary) batterScore.Fours--;
            if (lastBall.IsSix) batterScore.Sixes--;
        }

        // Reverse bowling score
        var bowlerScore = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == lastBall.BowlerId);
        if (bowlerScore != null)
        {
            bowlerScore.RunsConceded -= lastBall.TotalRuns;
            if (lastBall.IsLegalDelivery) bowlerScore.BallsBowled--;
            if (lastBall.ExtraType == ExtraType.Wide) bowlerScore.Wides--;
            if (lastBall.ExtraType == ExtraType.NoBall) bowlerScore.NoBalls--;
            if (lastBall.TotalRuns == 0 && lastBall.IsLegalDelivery) bowlerScore.DotBalls--;
        }

        // Reverse wicket
        if (lastBall.IsWicket)
        {
            innings.TotalWickets--;
            var dismissedId = lastBall.DismissedPlayerId ?? lastBall.StrikerId;
            var dismissedScore = innings.BattingScorecard.FirstOrDefault(b => b.PlayerId == dismissedId);
            if (dismissedScore != null)
            {
                dismissedScore.IsOut = false;
                dismissedScore.WicketType = WicketType.None;
                dismissedScore.DismissalText = "not out";
                dismissedScore.IsCurrentBatter = true;
            }
            if (bowlerScore != null && lastBall.WicketType != WicketType.RunOut)
                bowlerScore.Wickets--;
            if (innings.FallOfWickets.Count > 0)
                innings.FallOfWickets.RemoveAt(innings.FallOfWickets.Count - 1);
        }

        // Remove ball from over and innings
        innings.AllBalls.RemoveAt(innings.AllBalls.Count - 1);
        if (currentOver != null)
        {
            currentOver.Balls.RemoveAt(currentOver.Balls.Count - 1);
            currentOver.IsComplete = false;
            if (currentOver.Balls.Count == 0)
                innings.Overs.Remove(currentOver);
        }

        // Restore striker/non-striker from ball
        innings.CurrentStrikerId = lastBall.StrikerId;
        innings.CurrentNonStrikerId = lastBall.NonStrikerId;
        innings.CurrentBowlerId = lastBall.BowlerId;

        await SaveState();
        NotifyStateChanged();
    }

    private void UpdateBattingScore(Innings innings, BallEvent ball)
    {
        if (ball.ExtraType == ExtraType.Wide) return; // Wide doesn't count for batter

        var batterScore = innings.BattingScorecard.FirstOrDefault(b => b.PlayerId == ball.StrikerId);
        if (batterScore == null) return;

        if (ball.ExtraType != ExtraType.Bye && ball.ExtraType != ExtraType.LegBye)
        {
            batterScore.RunsScored += ball.RunsScored;
            if (ball.IsBoundary) batterScore.Fours++;
            if (ball.IsSix) batterScore.Sixes++;
        }

        if (ball.IsLegalDelivery || ball.ExtraType == ExtraType.NoBall)
            batterScore.BallsFaced++;
    }

    private void UpdateBowlingScore(Innings innings, BallEvent ball)
    {
        var bowlerScore = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == ball.BowlerId);
        if (bowlerScore == null) return;

        bowlerScore.RunsConceded += ball.TotalRuns;
        if (ball.IsLegalDelivery) bowlerScore.BallsBowled++;
        if (ball.ExtraType == ExtraType.Wide) bowlerScore.Wides++;
        if (ball.ExtraType == ExtraType.NoBall) bowlerScore.NoBalls++;
        if (ball.TotalRuns == 0 && ball.IsLegalDelivery) bowlerScore.DotBalls++;
        if (ball.IsWicket && ball.WicketType != WicketType.RunOut) bowlerScore.Wickets++;
    }

    private void RotateStrike(Innings innings)
    {
        (innings.CurrentStrikerId, innings.CurrentNonStrikerId) =
            (innings.CurrentNonStrikerId, innings.CurrentStrikerId);

        foreach (var bs in innings.BattingScorecard.Where(b => b.IsCurrentBatter))
        {
            bs.IsOnStrike = bs.PlayerId == innings.CurrentStrikerId;
        }
    }

    public void SetStriker(string playerId)
    {
        var innings = GetCurrentInnings();
        if (innings == null) return;
        innings.CurrentStrikerId = playerId;
        foreach (var bs in innings.BattingScorecard.Where(b => b.IsCurrentBatter))
            bs.IsOnStrike = bs.PlayerId == playerId;
        NotifyStateChanged();
    }

    public void SetNonStriker(string playerId)
    {
        var innings = GetCurrentInnings();
        if (innings == null) return;
        innings.CurrentNonStrikerId = playerId;
        NotifyStateChanged();
    }

    public async Task SetNewBatter(string playerId, string playerName)
    {
        var innings = GetCurrentInnings();
        if (innings == null) return;

        var battingOrder = innings.BattingScorecard.Count + 1;
        var newBatter = new BattingScore
        {
            PlayerId = playerId,
            PlayerName = playerName,
            BattingOrder = battingOrder,
            IsCurrentBatter = true,
            IsOnStrike = true
        };

        innings.BattingScorecard.Add(newBatter);
        innings.CurrentStrikerId = playerId;
        innings.YetToBat.Remove(playerId);

        foreach (var bs in innings.BattingScorecard.Where(b => b.IsCurrentBatter && b.PlayerId != playerId))
            bs.IsOnStrike = false;

        await SaveState();
        NotifyStateChanged();
    }

    public async Task SetNewBowler(string playerId, string playerName)
    {
        var innings = GetCurrentInnings();
        if (innings == null) return;

        innings.CurrentBowlerId = playerId;

        var existing = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == playerId);
        if (existing == null)
        {
            innings.BowlingScorecard.Add(new BowlingScore
            {
                PlayerId = playerId,
                PlayerName = playerName
            });
        }

        await SaveState();
        NotifyStateChanged();
    }

    public bool IsOverComplete()
    {
        var innings = GetCurrentInnings();
        if (innings == null) return false;
        var currentOver = innings.Overs.LastOrDefault();
        return currentOver?.IsComplete ?? false;
    }

    public bool IsInningsComplete()
    {
        if (CurrentMatch == null) return false;
        var innings = GetCurrentInnings();
        if (innings == null) return false;

        // All out
        var battingTeamPlayerCount = CurrentMatch.CurrentInningsNumber == 1
            ? (CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId ? CurrentMatch.TeamAPlayerIds.Count : CurrentMatch.TeamBPlayerIds.Count)
            : (CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId ? CurrentMatch.TeamBPlayerIds.Count : CurrentMatch.TeamAPlayerIds.Count);

        if (innings.TotalWickets >= battingTeamPlayerCount - 1)
            return true;

        // Overs complete
        int maxBalls = CurrentMatch.TotalOvers * 6;
        if (innings.TotalBalls >= maxBalls)
            return true;

        // Target reached in 2nd innings
        if (CurrentMatch.CurrentInningsNumber == 2 && innings.Target > 0 && innings.TotalRuns >= innings.Target)
            return true;

        return false;
    }

    public async Task EndInnings()
    {
        if (CurrentMatch == null) return;
        var innings = GetCurrentInnings();
        if (innings == null) return;

        innings.IsCompleted = true;

        if (CurrentMatch.CurrentInningsNumber == 1)
        {
            CurrentMatch.Status = MatchStatus.InningsBreak;
            CurrentMatch.CurrentInningsNumber = 2;

            // Setup second innings
            var battingTeamId = CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
                ? CurrentMatch.TeamBId : CurrentMatch.TeamAId;
            var bowlingTeamId = CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
                ? CurrentMatch.TeamAId : CurrentMatch.TeamBId;

            CurrentMatch.SecondInnings = new Innings
            {
                InningsNumber = 2,
                BattingTeamId = battingTeamId,
                BowlingTeamId = bowlingTeamId,
                Target = CurrentMatch.FirstInnings!.TotalRuns + 1
            };

            // Set yet to bat
            var battingPlayerIds = battingTeamId == CurrentMatch.TeamAId
                ? CurrentMatch.TeamAPlayerIds : CurrentMatch.TeamBPlayerIds;
            CurrentMatch.SecondInnings.YetToBat = new List<string>(battingPlayerIds);
        }
        else
        {
            await DetermineMatchResult();
        }

        await SaveState();
        await _matchService.SaveMatchAsync(CurrentMatch);
        NotifyStateChanged();
    }

    public async Task EndMatch()
    {
        if (CurrentMatch == null) return;
        await DetermineMatchResult();
        await SaveState();
        await _matchService.SaveMatchAsync(CurrentMatch);
        NotifyStateChanged();
    }

    public async Task AbandonMatch()
    {
        if (CurrentMatch == null) return;
        CurrentMatch.Status = MatchStatus.Abandoned;
        CurrentMatch.CompletedAt = DateTime.Now;
        CurrentMatch.Result = new MatchResult
        {
            ResultType = MatchResultType.Abandoned,
            ResultSummary = "Match Abandoned"
        };
        await _matchService.SaveMatchAsync(CurrentMatch);
        await _matchService.ClearLiveMatchAsync();
        NotifyStateChanged();
    }

    private async Task DetermineMatchResult()
    {
        if (CurrentMatch == null) return;

        var first = CurrentMatch.FirstInnings;
        var second = CurrentMatch.SecondInnings;
        if (first == null || second == null) return;

        second.IsCompleted = true;

        string battingFirstTeamName = CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
            ? CurrentMatch.TeamAName : CurrentMatch.TeamBName;
        string battingSecondTeamName = CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
            ? CurrentMatch.TeamBName : CurrentMatch.TeamAName;
        string battingSecondTeamId = CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
            ? CurrentMatch.TeamBId : CurrentMatch.TeamAId;

        if (second.TotalRuns > first.TotalRuns)
        {
            // Batting second team wins by wickets
            int wicketsRemaining = GetBattingTeamPlayerCount(2) - 1 - second.TotalWickets;
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.WonByWickets,
                WinnerTeamId = battingSecondTeamId,
                LoserTeamId = CurrentMatch.BattingFirstTeamId,
                WinMarginText = $"{wicketsRemaining} wicket{(wicketsRemaining != 1 ? "s" : "")}",
                ResultSummary = $"{battingSecondTeamName} won by {wicketsRemaining} wicket{(wicketsRemaining != 1 ? "s" : "")}"
            };
        }
        else if (first.TotalRuns > second.TotalRuns)
        {
            // Batting first team wins by runs
            int runMargin = first.TotalRuns - second.TotalRuns;
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.WonByRuns,
                WinnerTeamId = CurrentMatch.BattingFirstTeamId,
                LoserTeamId = battingSecondTeamId,
                WinMarginText = $"{runMargin} run{(runMargin != 1 ? "s" : "")}",
                ResultSummary = $"{battingFirstTeamName} won by {runMargin} run{(runMargin != 1 ? "s" : "")}"
            };
        }
        else
        {
            // Tie
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.Tie,
                ResultSummary = "Match Tied"
            };
        }

        CurrentMatch.Status = MatchStatus.Completed;
        CurrentMatch.CompletedAt = DateTime.Now;

        await _matchService.ClearLiveMatchAsync();
    }

    private int GetBattingTeamPlayerCount(int inningsNumber)
    {
        if (CurrentMatch == null) return 11;
        if (inningsNumber == 1)
        {
            return CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
                ? CurrentMatch.TeamAPlayerIds.Count
                : CurrentMatch.TeamBPlayerIds.Count;
        }
        else
        {
            return CurrentMatch.BattingFirstTeamId == CurrentMatch.TeamAId
                ? CurrentMatch.TeamBPlayerIds.Count
                : CurrentMatch.TeamAPlayerIds.Count;
        }
    }

    private string GetDismissalText(WicketType wicketType, string? bowlerId, string? fielderId, Innings innings)
    {
        var bowlerName = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == bowlerId)?.PlayerName ?? "";
        var fielderName = fielderId != null
            ? innings.BattingScorecard.Concat(innings.BowlingScorecard.Select(b => new BattingScore { PlayerId = b.PlayerId, PlayerName = b.PlayerName }))
                .FirstOrDefault(p => p.PlayerId == fielderId)?.PlayerName ?? ""
            : "";

        return wicketType switch
        {
            WicketType.Bowled => $"b {bowlerName}",
            WicketType.Caught => string.IsNullOrEmpty(fielderName) ? $"c & b {bowlerName}" : $"c {fielderName} b {bowlerName}",
            WicketType.LBW => $"lbw b {bowlerName}",
            WicketType.RunOut => string.IsNullOrEmpty(fielderName) ? "run out" : $"run out ({fielderName})",
            WicketType.Stumped => $"st {fielderName} b {bowlerName}",
            WicketType.HitWicket => $"hit wicket b {bowlerName}",
            WicketType.Retired => "retired out",
            WicketType.RetiredHurt => "retired hurt",
            _ => "out"
        };
    }

    // Super Over
    public async Task StartSuperOver()
    {
        if (CurrentMatch == null) return;

        CurrentMatch.Status = MatchStatus.SuperOver;
        CurrentMatch.SuperOver = new SuperOver();

        await SaveState();
        NotifyStateChanged();
    }

    public async Task SetupSuperOverInnings(bool isTeamABatting, List<string> batterIds, string bowlerId)
    {
        if (CurrentMatch?.SuperOver == null) return;

        var battingTeamId = isTeamABatting ? CurrentMatch.TeamAId : CurrentMatch.TeamBId;
        var bowlingTeamId = isTeamABatting ? CurrentMatch.TeamBId : CurrentMatch.TeamAId;

        var innings = new Innings
        {
            InningsNumber = isTeamABatting ? 1 : 2,
            BattingTeamId = battingTeamId,
            BowlingTeamId = bowlingTeamId,
            Target = isTeamABatting ? 0 : (CurrentMatch.SuperOver.TeamAInnings?.TotalRuns ?? 0) + 1
        };

        if (isTeamABatting)
        {
            CurrentMatch.SuperOver.TeamABatterIds = batterIds;
            CurrentMatch.SuperOver.TeamABowlerId = bowlerId;
            CurrentMatch.SuperOver.TeamAInnings = innings;
        }
        else
        {
            CurrentMatch.SuperOver.TeamBBatterIds = batterIds;
            CurrentMatch.SuperOver.TeamBBowlerId = bowlerId;
            CurrentMatch.SuperOver.TeamBInnings = innings;
        }

        await SaveState();
        NotifyStateChanged();
    }

    public async Task CompleteSuperOver()
    {
        if (CurrentMatch?.SuperOver == null) return;

        var soA = CurrentMatch.SuperOver.TeamAInnings;
        var soB = CurrentMatch.SuperOver.TeamBInnings;

        if (soA == null || soB == null) return;

        CurrentMatch.SuperOver.IsCompleted = true;

        if (soA.TotalRuns > soB.TotalRuns)
        {
            CurrentMatch.SuperOver.WinnerTeamId = CurrentMatch.TeamAId;
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.WonByRuns,
                WinnerTeamId = CurrentMatch.TeamAId,
                LoserTeamId = CurrentMatch.TeamBId,
                WasSuperOver = true,
                ResultSummary = $"{CurrentMatch.TeamAName} won the Super Over"
            };
        }
        else if (soB.TotalRuns > soA.TotalRuns)
        {
            CurrentMatch.SuperOver.WinnerTeamId = CurrentMatch.TeamBId;
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.WonByWickets,
                WinnerTeamId = CurrentMatch.TeamBId,
                LoserTeamId = CurrentMatch.TeamAId,
                WasSuperOver = true,
                ResultSummary = $"{CurrentMatch.TeamBName} won the Super Over"
            };
        }
        else
        {
            CurrentMatch.Result = new MatchResult
            {
                ResultType = MatchResultType.TiedSuperOver,
                WasSuperOver = true,
                ResultSummary = "Match tied (Super Over also tied)"
            };
        }

        CurrentMatch.Status = MatchStatus.Completed;
        CurrentMatch.CompletedAt = DateTime.Now;

        await _matchService.SaveMatchAsync(CurrentMatch);
        await _matchService.ClearLiveMatchAsync();
        NotifyStateChanged();
    }

    private async Task SaveState()
    {
        if (CurrentMatch != null)
            await _matchService.SaveLiveMatchAsync(CurrentMatch);
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}