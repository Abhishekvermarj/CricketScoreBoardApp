namespace CricketScoreboardApp.Services.Interfaces;

public interface IToastService
{
    event Action<string, string>? OnShow;
    void Show(string message, string type = "info");
    void Success(string message);
    void Error(string message);
    void Warning(string message);
}