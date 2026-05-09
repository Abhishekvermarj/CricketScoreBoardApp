using CricketScoreboardApp.Models.Enums;

namespace CricketScoreboardApp.Models;

public class BattingScore
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int RunsScored { get; set; }
    public int BallsFaced { get; set; }
    public int Fours { get; set; }
    public int Sixes { get; set; }
    public bool IsOut { get; set; }
    public WicketType WicketType { get; set; } = WicketType.None;
    public string? BowlerId { get; set; }
    public string? FielderId { get; set; }
    public string DismissalText { get; set; } = "not out";
    public bool IsOnStrike { get; set; }
    public bool IsCurrentBatter { get; set; }
    public int BattingOrder { get; set; }

    public double StrikeRate => BallsFaced > 0 ? Math.Round((double)RunsScored / BallsFaced * 100, 2) : 0;
}