namespace CricketScoreboardApp.Models;

public class SuperOver
{
    public Innings? TeamAInnings { get; set; }
    public Innings? TeamBInnings { get; set; }
    public List<string> TeamABatterIds { get; set; } = new();
    public List<string> TeamBBatterIds { get; set; } = new();
    public string? TeamABowlerId { get; set; }
    public string? TeamBBowlerId { get; set; }
    public bool IsCompleted { get; set; }
    public string? WinnerTeamId { get; set; }
}