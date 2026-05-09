using CricketScoreboardApp.Models.Enums;

namespace CricketScoreboardApp.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public PlayerRole Role { get; set; } = PlayerRole.Batter;
    public BattingStyle BattingStyle { get; set; } = BattingStyle.RightHanded;
    public BowlingStyle BowlingStyle { get; set; } = BowlingStyle.None;
    public string? TeamId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string RoleDisplay => Role switch
    {
        PlayerRole.Batter => "🏏 Batter",
        PlayerRole.Bowler => "🎯 Bowler",
        PlayerRole.AllRounder => "⚡ All-Rounder",
        PlayerRole.WicketKeeper => "🧤 Wicket-Keeper",
        _ => Role.ToString()
    };
}