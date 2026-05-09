using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}