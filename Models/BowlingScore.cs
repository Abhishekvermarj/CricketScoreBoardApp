namespace CricketScoreboardApp.Models;

public class BowlingScore
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int Overs { get; set; }
    public int BallsBowled { get; set; }
    public int Maidens { get; set; }
    public int RunsConceded { get; set; }
    public int Wickets { get; set; }
    public int Wides { get; set; }
    public int NoBalls { get; set; }
    public int DotBalls { get; set; }

    public string OversDisplay
    {
        get
        {
            int completedOvers = BallsBowled / 6;
            int remainingBalls = BallsBowled % 6;
            return remainingBalls > 0 ? $"{completedOvers}.{remainingBalls}" : $"{completedOvers}";
        }
    }

    public double Economy => BallsBowled > 0 ? Math.Round((double)RunsConceded / BallsBowled * 6, 2) : 0;
}