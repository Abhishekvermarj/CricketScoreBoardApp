using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface ITeamService
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<Team?> GetTeamByIdAsync(string id);
    Task AddTeamAsync(Team team);
    Task UpdateTeamAsync(Team team);
    Task DeleteTeamAsync(string id);
    Task AddPlayerToTeamAsync(string teamId, string playerId);
    Task RemovePlayerFromTeamAsync(string teamId, string playerId);
}