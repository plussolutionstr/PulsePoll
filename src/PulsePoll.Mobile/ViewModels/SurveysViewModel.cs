using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class SurveysViewModel : ObservableObject
{
    private readonly MockDataService _dataService;

    public SurveysViewModel(MockDataService dataService)
    {
        _dataService = dataService;
        Surveys = new ObservableCollection<SurveyModel>(_dataService.GetSurveys());
    }

    [ObservableProperty] private ObservableCollection<SurveyModel> _surveys = [];

    [RelayCommand]
    private async Task NavigateToSurveyDetail(SurveyModel survey)
    {
        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }
}
