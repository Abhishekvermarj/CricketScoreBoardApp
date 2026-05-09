namespace CricketScoreboardApp.Helpers;

public static class CricketHelpers
{
    public static string FormatOvers(int totalBalls)
    {
        int overs = totalBalls / 6;
        int balls = totalBalls % 6;
        return balls > 0 ? $"{overs}.{balls}" : $"{overs}";
    }

    public static double CalculateRunRate(int runs, int balls)
    {
        if (balls == 0) return 0;
        return Math.Round((double)runs / balls * 6, 2);
    }

    public static double CalculateRequiredRunRate(int runsNeeded, int ballsRemaining)
    {
        if (ballsRemaining <= 0) return 0;
        return Math.Round((double)runsNeeded / ballsRemaining * 6, 2);
    }

    public static string GetResultColor(string? winnerTeamId, string teamId)
    {
        if (winnerTeamId == null) return "var(--text-muted)";
        return winnerTeamId == teamId ? "var(--accent)" : "var(--danger)";
    }
}