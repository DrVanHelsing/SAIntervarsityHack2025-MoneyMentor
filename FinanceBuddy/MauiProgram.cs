using Microsoft.Extensions.Logging;
using FinanceBuddy.Services;
using FinanceBuddy.Pages;
using CommunityToolkit.Maui;

namespace FinanceBuddy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register services for dependency injection
            builder.Services.AddHttpClient<ApiClient>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddSingleton<IGamificationService, GamificationService>();
            
            // Register pages and shell
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<ExpensesPage>();
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<GamificationTestPage>();

            var app = builder.Build();
            ServiceHelper.Initialize(app.Services);
            return app;
        }
    }
}
