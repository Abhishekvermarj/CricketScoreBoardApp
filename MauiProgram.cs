using CricketScoreboardApp.Services;
using CricketScoreboardApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CricketScoreboardApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // App services used by Razor pages/components.
        builder.Services.AddSingleton<ILocalStorageService, JsonLocalStorageService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITeamService, TeamService>();
        builder.Services.AddSingleton<IPlayerService, PlayerService>();
        builder.Services.AddSingleton<IMatchService, MatchService>();
        builder.Services.AddSingleton<IRecordService, RecordService>();
        builder.Services.AddSingleton<IToastService, ToastService>();
        builder.Services.AddSingleton<LiveMatchState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
