namespace CricketScoreboardApp.Models;

public class PlayerRecord
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int TotalInnings { get; set; }
    public int TotalRuns { get; set; }
    public int HighestScore { get; set; }
    public bool HighestScoreNotOut { get; set; }
    public int TotalBallsFaced { get; set; }
    public int TotalFours { get; set; }
    public int TotalSixes { get; set; }
    public int TotalTimesOut { get; set; }
    public int TotalFifties { get; set; }
    public int TotalHundreds { get; set; }
    public double BattingAverage => TotalTimesOut > 0 ? Math.Round((double)TotalRuns / TotalTimesOut, 2) : TotalRuns;
    public double StrikeRate => TotalBallsFaced > 0 ? Math.Round((double)TotalRuns / TotalBallsFaced * 100, 2) : 0;

    // Bowling
    public int TotalOversBowled { get; set; }
    public int TotalBallsBowled { get; set; }
    public int TotalRunsConceded { get; set; }
    public int TotalWickets { get; set; }
    public int BestBowlingWickets { get; set; }
    public int BestBowlingRuns { get; set; }
    public int TotalMaidens { get; set; }
    public double BowlingAverage => TotalWickets > 0 ? Math.Round((double)TotalRunsConceded / TotalWickets, 2) : 0;
    public double EconomyRate => TotalBallsBowled > 0 ? Math.Round((double)TotalRunsConceded / TotalBallsBowled * 6, 2) : 0;

    public string BestBowling => TotalWickets > 0 ? $"{BestBowlingWickets}/{BestBowlingRuns}" : "-";
    public string HighestScoreDisplay => HighestScore > 0 ? $"{HighestScore}{(HighestScoreNotOut ? "*" : "")}" : "-";
}