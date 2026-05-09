namespace CricketScoreboardApp.Models;

public class Innings
{
    public int InningsNumber { get; set; }
    public string BattingTeamId { get; set; } = string.Empty;
    public string BowlingTeamId { get; set; } = string.Empty;
    public int TotalRuns { get; set; }
    public int TotalWickets { get; set; }
    public int TotalOvers { get; set; }
    public int TotalBalls { get; set; }
    public int Extras { get; set; }
    public int Wides { get; set; }
    public int NoBalls { get; set; }
    public int Byes { get; set; }
    public int LegByes { get; set; }
    public int Target { get; set; }
    public bool IsCompleted { get; set; }
    public List<Over> Overs { get; set; } = new();
    public List<BallEvent> AllBalls { get; set; } = new();
    public List<BattingScore> BattingScorecard { get; set; } = new();
    public List<BowlingScore> BowlingScorecard { get; set; } = new();
    public string? CurrentStrikerId { get; set; }
    public string? CurrentNonStrikerId { get; set; }
    public string? CurrentBowlerId { get; set; }
    public List<string> BattingOrder { get; set; } = new();
    public List<string> YetToBat { get; set; } = new();
    public List<string> FallOfWickets { get; set; } = new();

    public string OversDisplay
    {
        get
        {
            int completedOvers = TotalBalls / 6;
            int remainingBalls = TotalBalls % 6;
            return remainingBalls > 0 ? $"{completedOvers}.{remainingBalls}" : $"{completedOvers}";
        }
    }

    public string ScoreDisplay => $"{TotalRuns}/{TotalWickets}";
}