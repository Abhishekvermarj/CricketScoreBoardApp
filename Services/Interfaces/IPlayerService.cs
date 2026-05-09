using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface IPlayerService
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(string id);
    Task<List<Player>> GetPlayersByTeamIdAsync(string teamId);
    Task<List<Player>> GetPlayersByIdsAsync(List<string> ids);
    Task AddPlayerAsync(Player player);
    Task UpdatePlayerAsync(Player player);
    Task DeletePlayerAsync(string id);
}