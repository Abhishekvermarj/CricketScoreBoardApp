using CricketScoreboardApp.Services.Interfaces;

namespace CricketScoreboardApp.Services;

public class ToastService : IToastService
{
    public event Action<string, string>? OnShow;

    public void Show(string message, string type = "info") => OnShow?.Invoke(message, type);
    public void Success(string message) => OnShow?.Invoke(message, "success");
    public void Error(string message) => OnShow?.Invoke(message, "error");
    public void Warning(string message) => OnShow?.Invoke(message, "warning");
}