using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class ContactPage : ContentPage
{
    public ContactPage(ContactViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContactViewModel vm)
            vm.LoadContentCommand.Execute(null);
    }
}
