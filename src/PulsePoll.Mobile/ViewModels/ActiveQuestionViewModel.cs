using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(SurveyId), "surveyId")]
public partial class ActiveQuestionViewModel : ObservableObject
{
    private readonly MockDataService _dataService;
    private List<QuestionModel> _questions = [];

    public ActiveQuestionViewModel(MockDataService dataService)
    {
        _dataService = dataService;
    }

    [ObservableProperty] private int _surveyId;
    [ObservableProperty] private QuestionModel? _currentQuestion;
    [ObservableProperty] private ObservableCollection<OptionModel> _options = [];
    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private int _totalQuestions;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private int? _selectedOptionId;
    [ObservableProperty] private string _brandName = "";
    [ObservableProperty] private decimal _reward;
    [ObservableProperty] private bool _canGoBack;

    partial void OnSurveyIdChanged(int value)
    {
        var survey = _dataService.GetSurveyDetail(value);
        BrandName = survey.BrandName;
        Reward = survey.Reward;
        _questions = _dataService.GetQuestions();
        TotalQuestions = survey.QuestionCount;
        CurrentIndex = 0;
        LoadQuestion();
    }

    [RelayCommand]
    private void SelectOption(OptionModel option)
    {
        SelectedOptionId = option.Id;
    }

    [RelayCommand]
    private void NextQuestion()
    {
        if (SelectedOptionId == null) return;

        if (CurrentIndex < _questions.Count - 1)
        {
            CurrentIndex++;
            LoadQuestion();
        }
        else
        {
            Shell.Current.GoToAsync("../..");
        }
    }

    [RelayCommand]
    private void PreviousQuestion()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            LoadQuestion();
        }
    }

    [RelayCommand]
    private async Task CloseSurvey()
    {
        await Shell.Current.GoToAsync("../..");
    }

    private void LoadQuestion()
    {
        if (CurrentIndex < _questions.Count)
        {
            CurrentQuestion = _questions[CurrentIndex];
            Options = new ObservableCollection<OptionModel>(CurrentQuestion.Options);
        }
        SelectedOptionId = null;
        Progress = (double)(CurrentIndex + 1) / TotalQuestions;
        CanGoBack = CurrentIndex > 0;
    }
}
