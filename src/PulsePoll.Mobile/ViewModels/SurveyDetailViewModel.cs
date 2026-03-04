using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(SurveyId), "id")]
public partial class SurveyDetailViewModel : ObservableObject
{
    private readonly MockDataService _dataService;

    public SurveyDetailViewModel(MockDataService dataService)
    {
        _dataService = dataService;
    }

    [ObservableProperty] private int _surveyId;
    [ObservableProperty] private SurveyModel? _survey;
    [ObservableProperty] private ObservableCollection<SurveyCriteria> _criteria = [];

    partial void OnSurveyIdChanged(int value)
    {
        Survey = _dataService.GetSurveyDetail(value);
        Criteria = new ObservableCollection<SurveyCriteria>(Survey.Criteria);
    }

    [RelayCommand]
    private async Task StartSurvey()
    {
        await Shell.Current.GoToAsync($"activequestion?surveyId={SurveyId}");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
