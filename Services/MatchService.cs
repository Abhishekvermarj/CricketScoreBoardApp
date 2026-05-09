using CricketScoreboardApp.Helpers;
using CricketScoreboardApp.Models;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class MatchService : IMatchService
{
    private readonly ILocalStorageService _storage;
    private List<Match>? _cache;

    public MatchService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<List<Match>> GetAllMatchesAsync()
    {
        _cache ??= await _storage.LoadAsync<List<Match>>(StorageConstants.MatchesFile) ?? new List<Match>();
        return _cache;
    }

    public async Task<Match?> GetMatchByIdAsync(string id)
    {
        var matches = await GetAllMatchesAsync();
        return matches.FirstOrDefault(m => m.Id == id);
    }

    public async Task<List<Match>> GetMatchesByTeamIdAsync(string teamId)
    {
        var matches = await GetAllMatchesAsync();
        return matches.Where(m => m.TeamAId == teamId || m.TeamBId == teamId)
                      .OrderByDescending(m => m.CreatedAt)
                      .ToList();
    }

    public async Task SaveMatchAsync(Match match)
    {
        var matches = await GetAllMatchesAsync();
        var index = matches.FindIndex(m => m.Id == match.Id);
        if (index >= 0)
            matches[index] = match;
        else
            matches.Add(match);

        _cache = matches;
        await _storage.SaveAsync(StorageConstants.MatchesFile, matches);
    }

    public async Task DeleteMatchAsync(string id)
    {
        var matches = await GetAllMatchesAsync();
        matches.RemoveAll(m => m.Id == id);
        _cache = matches;
        await _storage.SaveAsync(StorageConstants.MatchesFile, matches);
    }

    public async Task SaveLiveMatchAsync(Match match)
    {
        await _storage.SaveAsync(StorageConstants.LiveMatchFile, match);
    }

    public async Task<Match?> LoadLiveMatchAsync()
    {
        return await _storage.LoadAsync<Match>(StorageConstants.LiveMatchFile);
    }

    public async Task ClearLiveMatchAsync()
    {
        await _storage.DeleteAsync(StorageConstants.LiveMatchFile);
    }
}