namespace CricketScoreboardApp.Models;

public class Team
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Color { get; set; } = "#1a73e8";
    public List<string> PlayerIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}