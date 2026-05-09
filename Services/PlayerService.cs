using CricketScoreboardApp.Helpers;
using CricketScoreboardApp.Models;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class PlayerService : IPlayerService
{
    private readonly ILocalStorageService _storage;
    private List<Player>? _cache;

    public PlayerService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        _cache ??= await _storage.LoadAsync<List<Player>>(StorageConstants.PlayersFile) ?? new List<Player>();
        return _cache;
    }

    public async Task<Player?> GetPlayerByIdAsync(string id)
    {
        var players = await GetAllPlayersAsync();
        return players.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<Player>> GetPlayersByTeamIdAsync(string teamId)
    {
        var players = await GetAllPlayersAsync();
        return players.Where(p => p.TeamId == teamId).ToList();
    }

    public async Task<List<Player>> GetPlayersByIdsAsync(List<string> ids)
    {
        var players = await GetAllPlayersAsync();
        return players.Where(p => ids.Contains(p.Id)).ToList();
    }

    public async Task AddPlayerAsync(Player player)
    {
        var players = await GetAllPlayersAsync();
        players.Add(player);
        await SaveAsync(players);
    }

    public async Task UpdatePlayerAsync(Player player)
    {
        var players = await GetAllPlayersAsync();
        var index = players.FindIndex(p => p.Id == player.Id);
        if (index >= 0)
        {
            players[index] = player;
            await SaveAsync(players);
        }
    }

    public async Task DeletePlayerAsync(string id)
    {
        var players = await GetAllPlayersAsync();
        players.RemoveAll(p => p.Id == id);
        await SaveAsync(players);
    }

    private async Task SaveAsync(List<Player> players)
    {
        _cache = players;
        await _storage.SaveAsync(StorageConstants.PlayersFile, players);
    }
}