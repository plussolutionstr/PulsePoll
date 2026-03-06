using PulsePoll.Mobile.Services;
using PulsePoll.Mobile.Views;

namespace PulsePoll.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var tokenProvider = _serviceProvider.GetRequiredService<ITokenProvider>();

        if (tokenProvider.HasTokens())
        {
            var window = new Window(new ContentPage());
            _ = TryAutoLoginAsync(window);
            return window;
        }

        return new Window(CreateAuthNavigation());
    }

    private async Task TryAutoLoginAsync(Window window)
    {
        try
        {
            var apiClient = _serviceProvider.GetRequiredService<IPulsePollApiClient>();
            var refreshed = await apiClient.TryRefreshSessionAsync();

            if (refreshed)
            {
                var pushService = _serviceProvider.GetRequiredService<IPushNotificationService>();
                _ = pushService.RegisterAsync();
                window.Page = _serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                window.Page = CreateAuthNavigation();
            }
        }
        catch
        {
            window.Page = CreateAuthNavigation();
        }
    }

    private NavigationPage CreateAuthNavigation()
    {
        var welcomePage = _serviceProvider.GetRequiredService<WelcomePage>();
        return new NavigationPage(welcomePage)
        {
            BarBackgroundColor = Color.FromArgb("#F7F5FF"),
            BarTextColor = Color.FromArgb("#1A1535")
        };
    }
}
