namespace PulsePoll.Mobile.Controls;

public partial class ConnectionErrorOverlay : ContentView
{
    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(Command), typeof(ConnectionErrorOverlay));

    public Command? RetryCommand
    {
        get => (Command?)GetValue(RetryCommandProperty);
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
            RetryCommand.Execute(null);
            // Give the command time to complete and hide the overlay
            await Task.Delay(500);
        }
        finally
        {
            RetryButton.IsVisible = true;
            RetrySpinner.IsVisible = false;
            RetrySpinner.IsRunning = false;
        }
    }
}
