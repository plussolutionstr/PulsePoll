using PulsePoll.Mobile.ViewModels;
using System.ComponentModel;

namespace PulsePoll.Mobile.Views;

public partial class StoryViewerPage : ContentPage
{
    private readonly StoryViewerViewModel _viewModel;
    private bool _isStoryAnimating;
    private DateTimeOffset _navigationPressStartedAt;

    private static readonly TimeSpan NavigationTapThreshold = TimeSpan.FromMilliseconds(220);

    public StoryViewerPage(StoryViewerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.ResumeProgress();
    }

    protected override void OnDisappearing()
    {
        _viewModel.PauseProgress();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnDisappearing();
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StoryViewerViewModel.CurrentStory))
            return;

        await AnimateStoryChangeAsync();
    }

    private async Task AnimateStoryChangeAsync()
    {
        if (_isStoryAnimating || StoryVisual is null)
            return;

        _isStoryAnimating = true;
        try
        {
            StoryVisual.Opacity = 0;
            StoryVisual.Scale = 1.03;

            await Task.WhenAll(
                StoryVisual.FadeToAsync(1, 260, Easing.CubicOut),
                StoryVisual.ScaleToAsync(1, 260, Easing.CubicOut));
        }
        finally
        {
            _isStoryAnimating = false;
        }
    }

    private void OnNavigationZonePressed(object? sender, EventArgs e)
    {
        _navigationPressStartedAt = DateTimeOffset.UtcNow;
        _viewModel.PauseProgress();
    }

    private async void OnNavigationZoneReleased(object? sender, EventArgs e)
    {
        var pressedFor = DateTimeOffset.UtcNow - _navigationPressStartedAt;
        var isTap = pressedFor <= NavigationTapThreshold;
        var zone = (sender as Button)?.CommandParameter as string;

        if (isTap)
        {
            if (zone == "prev")
                await _viewModel.PreviousCommand.ExecuteAsync(null);
            else
                await _viewModel.NextCommand.ExecuteAsync(null);
        }

        _viewModel.ResumeProgress();
    }
}
