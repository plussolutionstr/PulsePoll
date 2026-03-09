using Microsoft.Maui.Storage;

namespace PulsePoll.Mobile.Services;

public class SubjectSessionService : ISubjectSessionService
{
    private const string SurveyHelperEnabledKey = "subject_session_survey_helper_enabled";

    public bool IsSurveyHelperEnabled { get; private set; } =
        Preferences.Default.Get(SurveyHelperEnabledKey, false);

    public void UpdateHelperCapability(bool isEnabled)
    {
        IsSurveyHelperEnabled = isEnabled;
        Preferences.Default.Set(SurveyHelperEnabledKey, isEnabled);
    }

    public void Clear()
    {
        IsSurveyHelperEnabled = false;
        Preferences.Default.Remove(SurveyHelperEnabledKey);
    }
}
