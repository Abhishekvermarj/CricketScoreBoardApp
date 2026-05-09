using CricketScoreboardApp.Helpers;
using CricketScoreboardApp.Models;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class SettingsService : ISettingsService
{
    private readonly ILocalStorageService _storage;
    private AppSettings? _cache;

    public SettingsService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        _cache ??= await _storage.LoadAsync<AppSettings>(StorageConstants.SettingsFile) ?? new AppSettings();
        return _cache;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _cache = settings;
        await _storage.SaveAsync(StorageConstants.SettingsFile, settings);
    }
}