namespace CricketScoreboardApp.Services.Interfaces;

public interface ILocalStorageService
{
    Task<T?> LoadAsync<T>(string fileName) where T : class;
    Task SaveAsync<T>(string fileName, T data) where T : class;
    Task DeleteAsync(string fileName);
    Task<bool> FileExistsAsync(string fileName);
    string GetFilePath(string fileName);
}