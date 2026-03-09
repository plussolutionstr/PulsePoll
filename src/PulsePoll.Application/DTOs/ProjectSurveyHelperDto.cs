namespace PulsePoll.Application.DTOs;

public record ProjectSurveyHelperEntryDto(
    int Id,
    int ProjectId,
    string QuestionText,
    string HelpText,
    DateTime CreatedAt);

public record SaveProjectSurveyHelperEntryDto(
    int? Id,
    string QuestionText,
    string HelpText);

public record ProjectSurveyHelperMatchDto(
    bool Found,
    string Message);
