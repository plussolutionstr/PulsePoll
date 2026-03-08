using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PulsePoll.Mobile.Controls;

public partial class ConnectionErrorOverlay : ContentView
{
    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(ConnectionErrorOverlay));

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public ConnectionErrorOverlay()
    {
        InitializeComponent();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        if (RetryCommand is null || !RetryCommand.CanExecute(null))
            return;

        RetryButton.IsVisible = false;
        RetrySpinner.IsVisible = true;
        RetrySpinner.IsRunning = true;

        try
        {
            if (RetryCommand is IAsyncRelayCommand asyncRelayCommand)
                await asyncRelayCommand.ExecuteAsync(null);
            else
                RetryCommand.Execute(null);
        }
        finally
        {
            RetryButton.IsVisible = true;
            RetrySpinner.IsVisible = false;
            RetrySpinner.IsRunning = false;
        }
    }
}
