using Plugin.StoreReview;
using PulsePoll.Mobile.Controls;
using PulsePoll.Mobile.ViewModels;
using System.ComponentModel;
using Microsoft.Maui.Storage;

namespace PulsePoll.Mobile.Views;

public partial class HomePage : ContentPage
{
    private const string HomeRefreshRequiredKey = "home_refresh_required";
    private readonly HomeViewModel _viewModel;
    private CancellationTokenSource? _shimmerCts;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateShimmerState();
        await _viewModel.LoadNotificationBadgeCommand.ExecuteAsync(null);

        if (_viewModel.Stories.Count == 0)
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
        else if (_viewModel.NeedsRefreshOnReturn || Preferences.Default.Get(HomeRefreshRequiredKey, false))
        {
            Preferences.Default.Set(HomeRefreshRequiredKey, false);
            await _viewModel.RefreshCommand.ExecuteAsync(null);
        }

        TryShowRatePrompt();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShimmer();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HomeViewModel.IsLoading))
            return;

        MainThread.BeginInvokeOnMainThread(UpdateShimmerState);
    }

    private void UpdateShimmerState()
    {
        if (_viewModel.IsLoading)
        {
            StartShimmer();
            return;
        }

        StopShimmer();
        TryShowCoachMarks();
    }

    private bool _coachMarksTriggered;

    private async void TryShowCoachMarks()
    {
        if (_coachMarksTriggered || CoachMark.HasBeenShown)
            return;

        _coachMarksTriggered = true;

        // Wait for layout to settle — target must have non-zero size
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(100);
            if (CoachStories.Width > 0 && CoachStories.Height > 0)
                break;
        }

        var steps = new List<CoachMarkStep>
        {
            new()
            {
                Target = CoachStories,
                Title = "Hikayeler",
                Description = "Kampanya ve duyuruları buradan takip edebilirsin.",
                CornerRadius = 12
            },
            new()
            {
                Target = CoachNewsSlider,
                Title = "Haberler",
                Description = "En güncel haberleri ve duyuruları kaydırarak keşfet.",
                CornerRadius = 16
            },
            new()
            {
                Target = CoachSurveyHeader,
                Title = "Anketler",
                Description = "Sana özel anketleri tamamla, para kazan! Tümünü görmek için \"Tümü\"ne dokun.",
                CornerRadius = 8
            }
        };

        await CoachMark.ShowAsync(steps);
    }

    private static void TryShowRatePrompt()
    {
        if (Preferences.Default.Get("rate_prompt_shown", false))
            return;

        var installTicks = Preferences.Default.Get("app_install_date", 0L);
        if (installTicks <= 0)
            return;

        var installDate = new DateTime(installTicks, DateTimeKind.Utc);
        if ((DateTime.UtcNow - installDate).TotalDays < 30)
            return;

        Preferences.Default.Set("rate_prompt_shown", true);
        try
        {
            CrossStoreReview.Current.RequestReview(false);
        }
        catch
        {
            // Platform may not support in-app rating
        }
    }

    private void StartShimmer()
    {
        if (_shimmerCts is not null)
            return;

        _shimmerCts = new CancellationTokenSource();
        _ = RunShimmerAsync(_shimmerCts.Token);
    }

    private void StopShimmer()
    {
        if (_shimmerCts is null)
            return;

        _shimmerCts.Cancel();
        _shimmerCts.Dispose();
        _shimmerCts = null;

        ShimmerSweep.CancelAnimations();
        ShimmerSweep.Opacity = 0;
    }

    private async Task RunShimmerAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (ShimmerContainer.Width <= 0 || ShimmerSweep.Width <= 0)
                {
                    await Task.Delay(90, cancellationToken);
                    continue;
                }

                var startX = -ShimmerSweep.Width - 120;
                var endX = ShimmerContainer.Width + 120;

                ShimmerSweep.TranslationX = startX;
                ShimmerSweep.Opacity = 0;

                await ShimmerSweep.FadeToAsync(0.55, 120, Easing.CubicIn);
                await ShimmerSweep.TranslateToAsync(endX, 0, 1050, Easing.CubicInOut);
                await ShimmerSweep.FadeToAsync(0, 180, Easing.CubicOut);
                await Task.Delay(120, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation.
        }
    }
}
