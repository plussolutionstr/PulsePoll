using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PulsePoll.Mobile.ViewModels;

public partial class WelcomeViewModel(IServiceProvider serviceProvider) : ObservableObject
{
    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        var page = serviceProvider.GetRequiredService<Views.LoginPage>();
        await Application.Current!.Windows[0].Page!.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        var page = serviceProvider.GetRequiredService<Views.RegisterPage>();
        await Application.Current!.Windows[0].Page!.Navigation.PushAsync(page);
    }
}
