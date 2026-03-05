using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveysPage : ContentPage
{
    private readonly SurveysViewModel _viewModel;

    public SurveysPage(SurveysViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
