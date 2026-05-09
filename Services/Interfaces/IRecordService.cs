using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface IRecordService
{
    Task<List<PlayerRecord>> GetAllPlayerRecordsAsync();
    Task<PlayerRecord?> GetPlayerRecordAsync(string playerId);
}