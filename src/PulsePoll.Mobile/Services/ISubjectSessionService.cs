namespace PulsePoll.Mobile.Services;

public interface ISubjectSessionService
{
    bool IsSurveyHelperEnabled { get; }
    void UpdateHelperCapability(bool isEnabled);
    void Clear();
}
