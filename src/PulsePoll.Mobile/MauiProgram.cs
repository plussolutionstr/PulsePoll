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
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                });

                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                });

                Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                });

                Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                });
#elif IOS
                handlers.AddHandler<Entry, Microsoft.Maui.Handlers.EntryHandler>();
                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
                {
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                });

                handlers.AddHandler<Picker, Microsoft.Maui.Handlers.PickerHandler>();
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
                {
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                });

                handlers.AddHandler<DatePicker, Microsoft.Maui.Handlers.DatePickerHandler>();
                Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
                {
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                });
#endif
            });

        var settings = AppSettings.Load();
        builder.Services.AddSingleton(settings);

        // Shell & Auth
        builder.Services.AddTransient<AppShell>();

        // Services
#if ANDROID
        builder.Services.AddSingleton<IPushNotificationService, Platforms.Android.PushNotificationService>();
#elif IOS
        builder.Services.AddSingleton<IPushNotificationService, Platforms.iOS.PushNotificationService>();
#endif
        builder.Services.AddSingleton<NotificationCenterService>();
        builder.Services.AddSingleton<ITokenProvider, DevTokenProvider>();
        builder.Services.AddTransient<AuthDelegatingHandler>();

        builder.Services.AddHttpClient<IPulsePollApiClient, PulsePollApiClient>(client =>
        {
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
        })
        .AddHttpMessageHandler<AuthDelegatingHandler>()
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
#endif
        ;

        builder.Services.AddHttpClient<INotificationApiClient, NotificationApiClient>(client =>
        {
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
        })
        .AddHttpMessageHandler<AuthDelegatingHandler>()
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
#endif
        ;

        // ViewModels
        builder.Services.AddTransient<WelcomeViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SurveysViewModel>();
        builder.Services.AddTransient<SurveyDetailViewModel>();
        builder.Services.AddTransient<ActiveQuestionViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<WalletViewModel>();
        builder.Services.AddTransient<WalletAddBankAccountViewModel>();
        builder.Services.AddTransient<WalletWithdrawViewModel>();
        builder.Services.AddTransient<WalletTransactionsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<StoryViewerViewModel>();
        builder.Services.AddTransient<NewsDetailViewModel>();
        builder.Services.AddTransient<SurveyWebViewViewModel>();
        builder.Services.AddTransient<SurveyResultViewModel>();
        builder.Services.AddTransient<ConnectionErrorViewModel>();

        // Pages
        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SurveysPage>();
        builder.Services.AddTransient<SurveyDetailPage>();
        builder.Services.AddTransient<ActiveQuestionPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<WalletPage>();
        builder.Services.AddTransient<WalletAddBankAccountPage>();
        builder.Services.AddTransient<WalletWithdrawPage>();
        builder.Services.AddTransient<WalletTransactionsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<StoryViewerPage>();
        builder.Services.AddTransient<NewsDetailPage>();
        builder.Services.AddTransient<SurveyWebViewPage>();
        builder.Services.AddTransient<SurveyResultPage>();
        builder.Services.AddTransient<ConnectionErrorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
