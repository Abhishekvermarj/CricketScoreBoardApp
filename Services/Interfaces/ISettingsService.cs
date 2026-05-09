using CricketScoreboardApp.Models;

namespace CricketScoreboardApp.Services.Interfaces;

public interface ISettingsService
{
    event Action? OnSettingsChanged;
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}