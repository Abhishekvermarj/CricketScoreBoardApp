using CricketScoreboardApp.Helpers;
using CricketScoreboardApp.Models;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class TeamService : ITeamService
{
    private readonly ILocalStorageService _storage;
    private List<Team>? _cache;

    public TeamService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        _cache ??= await _storage.LoadAsync<List<Team>>(StorageConstants.TeamsFile) ?? new List<Team>();
        return _cache;
    }

    public async Task<Team?> GetTeamByIdAsync(string id)
    {
        var teams = await GetAllTeamsAsync();
        return teams.FirstOrDefault(t => t.Id == id);
    }

    public async Task AddTeamAsync(Team team)
    {
        var teams = await GetAllTeamsAsync();
        teams.Add(team);
        await SaveAsync(teams);
    }

    public async Task UpdateTeamAsync(Team team)
    {
        var teams = await GetAllTeamsAsync();
        var index = teams.FindIndex(t => t.Id == team.Id);
        if (index >= 0)
        {
            teams[index] = team;
            await SaveAsync(teams);
        }
    }

    public async Task DeleteTeamAsync(string id)
    {
        var teams = await GetAllTeamsAsync();
        teams.RemoveAll(t => t.Id == id);
        await SaveAsync(teams);
    }

    public async Task AddPlayerToTeamAsync(string teamId, string playerId)
    {
        var team = await GetTeamByIdAsync(teamId);
        if (team != null && !team.PlayerIds.Contains(playerId))
        {
            team.PlayerIds.Add(playerId);
            await UpdateTeamAsync(team);
        }
    }

    public async Task RemovePlayerFromTeamAsync(string teamId, string playerId)
    {
        var team = await GetTeamByIdAsync(teamId);
        if (team != null)
        {
            team.PlayerIds.Remove(playerId);
            await UpdateTeamAsync(team);
        }
    }

    private async Task SaveAsync(List<Team> teams)
    {
        _cache = teams;
        await _storage.SaveAsync(StorageConstants.TeamsFile, teams);
    }
}