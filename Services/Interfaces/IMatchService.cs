using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface IMatchService
{
    Task<List<Match>> GetAllMatchesAsync();
    Task<Match?> GetMatchByIdAsync(string id);
    Task<List<Match>> GetMatchesByTeamIdAsync(string teamId);
    Task SaveMatchAsync(Match match);
    Task DeleteMatchAsync(string id);
    Task SaveLiveMatchAsync(Match match);
    Task<Match?> LoadLiveMatchAsync();
    Task ClearLiveMatchAsync();
}