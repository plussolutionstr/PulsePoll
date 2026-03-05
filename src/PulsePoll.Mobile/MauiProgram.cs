using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PulsePoll.Mobile.Services;
using PulsePoll.Mobile.ViewModels;
using PulsePoll.Mobile.Views;

namespace PulsePoll.Mobile;

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
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
                fonts.AddFont("PlusJakartaSans-ExtraBold.ttf", "JakartaExtraBold");
            });

        // Shell & Auth
        builder.Services.AddTransient<AppShell>();

        // Services
        builder.Services.AddSingleton<MockDataService>();
        builder.Services.AddSingleton<ITokenProvider, DevTokenProvider>();

        builder.Services.AddHttpClient<IPulsePollApiClient, PulsePollApiClient>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("https://10.0.2.2:5001");
#else
            client.BaseAddress = new Uri("https://localhost:5001");
#endif
        })
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
#endif
        ;

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SurveysViewModel>();
        builder.Services.AddTransient<SurveyDetailViewModel>();
        builder.Services.AddTransient<ActiveQuestionViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<WalletViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<StoryViewerViewModel>();
        builder.Services.AddTransient<NewsDetailViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SurveysPage>();
        builder.Services.AddTransient<SurveyDetailPage>();
        builder.Services.AddTransient<ActiveQuestionPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<WalletPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<StoryViewerPage>();
        builder.Services.AddTransient<NewsDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
