using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(SurveyId), "id")]
public partial class SurveyDetailViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private const string LocalSubjectPublicIdKey = "subject_public_id";
    private const string HomeRefreshRequiredKey = "home_refresh_required";

    public SurveyDetailViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private int _surveyId;
    [ObservableProperty] private SurveyModel? _survey;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isStarting;

    partial void OnSurveyIdChanged(int value)
    {
        _ = LoadSurveyAsync(value);
    }

    [RelayCommand]
    private async Task StartSurvey()
    {
        if (Survey is null || IsStarting)
            return;

        IsStarting = true;
        try
        {
            string targetUrl;
            try
            {
                targetUrl = await _apiClient.StartProjectAsync(Survey.Id);
            }
            catch
            {
                targetUrl = BuildFallbackStartUrl(Survey);
            }

            if (string.IsNullOrWhiteSpace(targetUrl))
                return;

            Preferences.Default.Set(HomeRefreshRequiredKey, true);
            var title = Uri.EscapeDataString(Survey.Title);
            var encodedUrl = Uri.EscapeDataString(targetUrl);
            await Shell.Current.GoToAsync($"surveywebview?projectId={Survey.Id}&title={title}&url={encodedUrl}");
        }
        finally
        {
            IsStarting = false;
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    private async Task LoadSurveyAsync(int surveyId)
    {
        IsLoading = true;
        try
        {
            Survey = await _apiClient.GetProjectByIdAsync(surveyId);
            if (Survey is null)
                await Shell.Current.GoToAsync("connectionerror");
        }
        catch
        {
            await Shell.Current.GoToAsync("connectionerror");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string BuildFallbackStartUrl(SurveyModel survey)
    {
        if (string.IsNullOrWhiteSpace(survey.SurveyUrl))
            return string.Empty;

        var subjectPublicId = GetOrCreateLocalSubjectPublicId();
        var subjectParam = string.IsNullOrWhiteSpace(survey.SubjectParameterName)
            ? "uid"
            : survey.SubjectParameterName.Trim();
        var separator = survey.SurveyUrl.Contains('?') ? "&" : "?";
        return $"{survey.SurveyUrl}{separator}{subjectParam}={Uri.EscapeDataString(subjectPublicId)}";
    }

    private static string GetOrCreateLocalSubjectPublicId()
    {
        var existing = Preferences.Default.Get<string?>(LocalSubjectPublicIdKey, null);
        if (!string.IsNullOrWhiteSpace(existing))
            return existing;

        var generated = Guid.NewGuid().ToString("D");
        Preferences.Default.Set(LocalSubjectPublicIdKey, generated);
        return generated;
    }
}