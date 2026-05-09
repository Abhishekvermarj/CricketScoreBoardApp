using CricketScoreboardApp.Models.Enums;

namespace CricketScoreboardApp.Models;

public class MatchResult
{
    public MatchResultType ResultType { get; set; } = MatchResultType.None;
    public string? WinnerTeamId { get; set; }
    public string? LoserTeamId { get; set; }
    public string WinMarginText { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public bool WasSuperOver { get; set; }
}