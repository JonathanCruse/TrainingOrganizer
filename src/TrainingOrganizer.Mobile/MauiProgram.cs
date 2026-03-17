using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using TrainingOrganizer.UI.Services;

namespace TrainingOrganizer.Mobile;

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

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Configure HttpClient pointing to the API
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://your-api-domain.com")
        });

        // Add MudBlazor
        builder.Services.AddMudServices();

        // Add API clients
        builder.Services.AddUIServices();

        return builder.Build();
    }
}
