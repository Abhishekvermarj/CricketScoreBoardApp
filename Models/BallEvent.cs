using CricketScoreboardApp.Models.Enums;

namespace CricketScoreboardApp.Models;

public class BallEvent
{
    public int BallNumber { get; set; }
    public int OverNumber { get; set; }
    public string StrikerId { get; set; } = string.Empty;
    public string NonStrikerId { get; set; } = string.Empty;
    public string BowlerId { get; set; } = string.Empty;
    public int RunsScored { get; set; }
    public int ExtraRuns { get; set; }
    public ExtraType ExtraType { get; set; } = ExtraType.None;
    public bool IsWicket { get; set; }
    public WicketType WicketType { get; set; } = WicketType.None;
    public string? DismissedPlayerId { get; set; }
    public string? FielderId { get; set; }
    public bool IsLegalDelivery { get; set; } = true;
    public bool IsBoundary { get; set; }
    public bool IsSix { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public int TotalRuns => RunsScored + ExtraRuns;

    public string DisplayText
    {
        get
        {
            if (IsWicket) return "W";
            if (ExtraType == ExtraType.Wide) return $"Wd{(ExtraRuns > 1 ? "+" + (ExtraRuns - 1) : "")}";
            if (ExtraType == ExtraType.NoBall) return $"Nb{(RunsScored > 0 ? "+" + RunsScored : "")}";
            if (ExtraType == ExtraType.Bye) return $"B{ExtraRuns}";
            if (ExtraType == ExtraType.LegBye) return $"Lb{ExtraRuns}";
            return RunsScored.ToString();
        }
    }
}