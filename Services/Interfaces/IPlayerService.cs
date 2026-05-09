using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface IPlayerService
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(string id);
    [System.Obsolete("Players are now independent. Use GetAllPlayersAsync() instead. Team assignment happens during match creation.", true)]
    Task<List<Player>> GetPlayersByTeamIdAsync(string teamId);
    Task<List<Player>> GetPlayersByIdsAsync(List<string> ids);
    Task AddPlayerAsync(Player player);
    Task UpdatePlayerAsync(Player player);
    Task DeletePlayerAsync(string id);
}