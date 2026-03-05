using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record SurveyResultPatternDto(
    int Id,
    AssignmentStatus Status,
    string MatchPattern,
    int Order);

public record SurveyResultScriptDto(
    int Id,
    string Name,
    bool IsActive,
    List<SurveyResultPatternDto> Patterns);

public record SaveSurveyResultPatternDto(
    AssignmentStatus Status,
    string MatchPattern,
    int Order);

public record SaveSurveyResultScriptDto(
    int? Id,
    string Name,
    bool IsActive,
    List<SaveSurveyResultPatternDto> Patterns);
