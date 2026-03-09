namespace PulsePoll.Mobile.Services;

public interface ISurveyHelperService
{
    Task<SurveyHelpMatchResult> GetHelpAsync(int projectId, string questionText, CancellationToken ct = default);
}
