using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly MockDataService _dataService;

    public HomeViewModel(MockDataService dataService)
    {
        _dataService = dataService;
        LoadData();
    }

    [ObservableProperty] private ObservableCollection<StoryModel> _stories = [];
    [ObservableProperty] private ObservableCollection<NewsModel> _news = [];
    [ObservableProperty] private ObservableCollection<SurveyModel> _surveys = [];

    [RelayCommand]
    private async Task NavigateToSurveyDetail(SurveyModel survey)
    {
        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }

    [RelayCommand]
    private async Task NavigateToNotifications()
    {
        await Shell.Current.GoToAsync("notifications");
    }

    private void LoadData()
    {
        Stories = new ObservableCollection<StoryModel>(_dataService.GetStories());
        News = new ObservableCollection<NewsModel>(_dataService.GetNews());
        Surveys = new ObservableCollection<SurveyModel>(_dataService.GetSurveys());
    }
}
