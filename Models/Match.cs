using CricketScoreboardApp.Models.Enums;

namespace CricketScoreboardApp.Models;

public class Match
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string TeamAId { get; set; } = string.Empty;
    public string TeamBId { get; set; } = string.Empty;
    public string TeamAName { get; set; } = string.Empty;
    public string TeamBName { get; set; } = string.Empty;
    public List<string> TeamAPlayerIds { get; set; } = new();
    public List<string> TeamBPlayerIds { get; set; } = new();
    public int TotalOvers { get; set; }
    public string TossWinnerTeamId { get; set; } = string.Empty;
    public TossDecision TossDecision { get; set; }
    public string BattingFirstTeamId { get; set; } = string.Empty;
    public string BowlingFirstTeamId { get; set; } = string.Empty;
    public MatchStatus Status { get; set; } = MatchStatus.NotStarted;
    public int CurrentInningsNumber { get; set; } = 1;
    public Innings? FirstInnings { get; set; }
    public Innings? SecondInnings { get; set; }
    public SuperOver? SuperOver { get; set; }
    public MatchResult Result { get; set; } = new();
    public bool SuperOverEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    public Innings? CurrentInnings => CurrentInningsNumber == 1 ? FirstInnings : SecondInnings;

    public string StatusDisplay => Status switch
    {
        MatchStatus.NotStarted => "Not Started",
        MatchStatus.InProgress => "Live",
        MatchStatus.InningsBreak => "Innings Break",
        MatchStatus.Completed => "Completed",
        MatchStatus.Abandoned => "Abandoned",
        MatchStatus.SuperOver => "Super Over",
        _ => Status.ToString()
    };
}