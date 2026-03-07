using PulsePoll.Mobile.Controls;
using PulsePoll.Mobile.ViewModels;
using System.ComponentModel;

namespace PulsePoll.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;
    private CancellationTokenSource? _shimmerCts;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateShimmerState();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShimmer();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ProfileViewModel.IsLoading))
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
        await Task.Delay(600);

        var steps = new List<CoachMarkStep>
        {
            new()
            {
                Target = CoachStars,
                Title = "Yıldızların",
                Description = "Anketleri tamamladıkça yıldızın artar ve daha fazla anket alırsın.",
                CornerRadius = 8
            },
            new()
            {
                Target = CoachReferral,
                Title = "Referans Kodun",
                Description = "Bu kodu arkadaşlarınla paylaş, birlikte kazanın! Kopyalamak için dokun.",
                CornerRadius = 12
            },
            new()
            {
                Target = CoachStats,
                Title = "İstatistiklerin",
                Description = "Tamamladığın, elendiğin anketleri ve başarı oranını buradan takip et.",
                CornerRadius = 16
            }
        };

        await CoachMark.ShowAsync(steps);
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
