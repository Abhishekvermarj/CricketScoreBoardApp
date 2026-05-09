namespace CricketScoreboardApp.Models;

public class AppSettings
{
    public bool DarkMode { get; set; } = true;
    public bool SuperOverEnabled { get; set; } = true;
    public int DefaultOvers { get; set; } = 20;
    public bool AutoSave { get; set; } = true;
    public bool SoundEnabled { get; set; } = false;
    public bool VibrationEnabled { get; set; } = true;
    public string AppVersion { get; set; } = "1.0.0";
    public bool SinglePlayerBatting { get; set; } = false;
    public bool WideBallRunEnabled { get; set; } = true;
}