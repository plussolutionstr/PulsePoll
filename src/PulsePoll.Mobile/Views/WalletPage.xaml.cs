using PulsePoll.Mobile.Controls;
using PulsePoll.Mobile.ViewModels;
using System.ComponentModel;

namespace PulsePoll.Mobile.Views;

public partial class WalletPage : ContentPage
{
    private readonly WalletViewModel _viewModel;
    private CancellationTokenSource? _shimmerCts;

    public WalletPage(WalletViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateShimmerState();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShimmer();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(WalletViewModel.IsLoading))
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
            if (CoachBalanceCard.Width > 0 && CoachBalanceCard.Height > 0)
                break;
        }

        var steps = new List<CoachMarkStep>
        {
            new()
            {
                Target = CoachBalanceCard,
                Title = "Bakiyen",
                Description = "Çekilebilir bakiyen, puanın ve toplam kazancın burada görünür.",
                CornerRadius = 24
            },
            new()
            {
                Target = CoachActionButtons,
                Title = "Para Çek & Geçmiş",
                Description = "Bakiyeni banka hesabına aktarmak veya işlem geçmişini görmek için bu butonları kullan.",
                CornerRadius = 22
            },
            new()
            {
                Target = CoachAddBankAccount,
                Title = "Banka Hesabı",
                Description = "Para çekebilmek için önce bir banka hesabı eklemelisin.",
                CornerRadius = 14
            }
        };

        await CoachMark.ShowAsync(steps);
    }

    private VerticalStackLayout? _expandedPanel;

    private void OnBankAccountTapped(object? sender, TappedEventArgs e)
    {
        var border = sender as Border;
        var outerStack = border?.Content as VerticalStackLayout;
        if (outerStack is null) return;

        var panel = outerStack.Children.OfType<VerticalStackLayout>()
            .FirstOrDefault(v => v.StyleId == "DeletePanel");

        if (panel is null) return;

        if (_expandedPanel == panel)
        {
            panel.IsVisible = false;
            _expandedPanel = null;
            return;
        }

        if (_expandedPanel is not null)
            _expandedPanel.IsVisible = false;

        panel.IsVisible = true;
        _expandedPanel = panel;
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
