using System.Text.Json;
using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class JsonLocalStorageService : ILocalStorageService
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonLocalStorageService()
    {
        _basePath = FileSystem.AppDataDirectory;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public string GetFilePath(string fileName) => Path.Combine(_basePath, fileName);

    public async Task<T?> LoadAsync<T>(string fileName) where T : class
    {
        await _semaphore.WaitAsync();
        try
        {
            var filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON parse error for {fileName}: {ex.Message}");
            // Backup corrupt file
            var filePath = GetFilePath(fileName);
            var backupPath = GetFilePath($"{fileName}.corrupt.{DateTime.Now:yyyyMMddHHmmss}");
            if (File.Exists(filePath))
                File.Copy(filePath, backupPath, true);
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading {fileName}: {ex.Message}");
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync<T>(string fileName, T data) where T : class
    {
        await _semaphore.WaitAsync();
        try
        {
            var filePath = GetFilePath(fileName);
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving {fileName}: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task DeleteAsync(string fileName)
    {
        try
        {
            var filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting {fileName}: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string fileName)
    {
        var filePath = GetFilePath(fileName);
        return Task.FromResult(File.Exists(filePath));
    }
}