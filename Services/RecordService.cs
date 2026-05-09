using CricketScoreboardApp.Models;
using CricketScoreboardApp.Models.Enums;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class RecordService : IRecordService
{
    private readonly IMatchService _matchService;
    private readonly IPlayerService _playerService;
    private readonly ITeamService _teamService;

    public RecordService(IMatchService matchService, IPlayerService playerService, ITeamService teamService)
    {
        _matchService = matchService;
        _playerService = playerService;
        _teamService = teamService;
    }

    public async Task<List<PlayerRecord>> GetAllPlayerRecordsAsync()
    {
        var players = await _playerService.GetAllPlayersAsync();
        var matches = await _matchService.GetAllMatchesAsync();
        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed).ToList();
        var records = new List<PlayerRecord>();

        foreach (var player in players)
        {
            var record = await BuildPlayerRecord(player, completedMatches);
            records.Add(record);
        }

        return records;
    }

    public async Task<PlayerRecord?> GetPlayerRecordAsync(string playerId)
    {
        var player = await _playerService.GetPlayerByIdAsync(playerId);
        if (player == null) return null;

        var matches = await _matchService.GetAllMatchesAsync();
        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed).ToList();
        return await BuildPlayerRecord(player, completedMatches);
    }

    private async Task<PlayerRecord> BuildPlayerRecord(Player player, List<Match> completedMatches)
    {
        var team = player.TeamId != null ? await _teamService.GetTeamByIdAsync(player.TeamId) : null;
        var record = new PlayerRecord
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            TeamName = team?.Name ?? "Unassigned"
        };

        foreach (var match in completedMatches)
        {
            bool isInMatch = match.TeamAPlayerIds.Contains(player.Id) || match.TeamBPlayerIds.Contains(player.Id);
            if (!isInMatch) continue;

            record.TotalMatches++;

            // Check first innings
            ProcessInningsBatting(match.FirstInnings, player.Id, record);
            ProcessInningsBowling(match.FirstInnings, player.Id, record);

            // Check second innings
            ProcessInningsBatting(match.SecondInnings, player.Id, record);
            ProcessInningsBowling(match.SecondInnings, player.Id, record);
        }

        return record;
    }

    private void ProcessInningsBatting(Innings? innings, string playerId, PlayerRecord record)
    {
        if (innings == null) return;

        var battingScore = innings.BattingScorecard.FirstOrDefault(b => b.PlayerId == playerId);
        if (battingScore == null) return;

        record.TotalInnings++;
        record.TotalRuns += battingScore.RunsScored;
        record.TotalBallsFaced += battingScore.BallsFaced;
        record.TotalFours += battingScore.Fours;
        record.TotalSixes += battingScore.Sixes;

        if (battingScore.IsOut)
            record.TotalTimesOut++;

        if (battingScore.RunsScored > record.HighestScore)
        {
            record.HighestScore = battingScore.RunsScored;
            record.HighestScoreNotOut = !battingScore.IsOut;
        }

        if (battingScore.RunsScored >= 100) record.TotalHundreds++;
        else if (battingScore.RunsScored >= 50) record.TotalFifties++;
    }

    private void ProcessInningsBowling(Innings? innings, string playerId, PlayerRecord record)
    {
        if (innings == null) return;

        var bowlingScore = innings.BowlingScorecard.FirstOrDefault(b => b.PlayerId == playerId);
        if (bowlingScore == null) return;

        record.TotalBallsBowled += bowlingScore.BallsBowled;
        record.TotalRunsConceded += bowlingScore.RunsConceded;
        record.TotalWickets += bowlingScore.Wickets;
        record.TotalMaidens += bowlingScore.Maidens;

        if (bowlingScore.Wickets > record.BestBowlingWickets ||
            (bowlingScore.Wickets == record.BestBowlingWickets && bowlingScore.RunsConceded < record.BestBowlingRuns))
        {
            record.BestBowlingWickets = bowlingScore.Wickets;
            record.BestBowlingRuns = bowlingScore.RunsConceded;
        }
    }
}