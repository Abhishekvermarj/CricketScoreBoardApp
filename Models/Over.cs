namespace CricketScoreboardApp.Models;

public class Over
{
    public int OverNumber { get; set; }
    public string BowlerId { get; set; } = string.Empty;
    public List<BallEvent> Balls { get; set; } = new();
    public bool IsComplete { get; set; }
    public bool IsMaiden => IsComplete && Balls.Where(b => b.IsLegalDelivery).Sum(b => b.RunsScored) == 0
                            && !Balls.Any(b => b.ExtraType == Enums.ExtraType.Wide || b.ExtraType == Enums.ExtraType.NoBall);

    public int LegalBalls => Balls.Count(b => b.IsLegalDelivery);
    public int TotalRuns => Balls.Sum(b => b.TotalRuns);
}