using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.ApiModels;

public static class ApiDtoMapper
{
    public static StoryModel ToModel(this StoryApiDto dto)
        => new(
            dto.Id,
            dto.Title,
            dto.ImageUrl,
            dto.StoryImageUrl ?? dto.ImageUrl,
            dto.LinkUrl,
            dto.Description ?? string.Empty,
            IsSeen: dto.IsSeen);

    public static NewsModel ToModel(this NewsApiDto dto)
        => new(dto.Id, "", dto.Title, dto.Summary, ImageUrl: dto.ImageUrl, LinkUrl: dto.LinkUrl);

    public static HistoryItemModel ToModel(this ProjectHistoryApiDto dto)
        => new(
            dto.ProjectId,
            dto.ProjectName,
            ToHistoryStatus(dto.Status),
            dto.EarnedAmount > 0m ? dto.EarnedAmount : null,
            dto.CompletedAt ?? dto.AssignedAt,
            0,
            dto.RewardUnitLabel,
            dto.DurationMinutes,
            dto.RewardAmount,
            dto.ConsolationRewardAmount);

    public static SurveyModel ToModel(this ProjectApiDto dto)
        => new(
            dto.Id,
            dto.CustomerShortName,
            dto.CoverImageUrl ?? "",
            dto.Name,
            dto.Description ?? "",
            dto.Category ?? "",
            dto.Reward,
            dto.EstimatedMinutes,
            0,
            [],
            ConsolationReward: dto.ConsolationReward,
            RewardUnitLabel: dto.RewardUnitLabel,
            SurveyUrl: dto.SurveyUrl,
            SubjectParameterName: dto.SubjectParameterName,
            StartMessage: dto.StartMessage,
            CompletedMessage: dto.CompletedMessage,
            DisqualifyMessage: dto.DisqualifyMessage,
            QuotaFullMessage: dto.QuotaFullMessage,
            ScreenOutMessage: dto.ScreenOutMessage,
            AssignmentStatus: dto.AssignmentStatus?.ToString(),
            ResultPatterns: dto.SurveyResultPatterns?
                .OrderBy(p => p.Order)
                .Select(p => new SurveyResultPatternModel(
                    p.Status.ToString(),
                    p.MatchPattern,
                    p.Order))
                .ToList());

    private static string ToHistoryStatus(AssignmentStatus status) => status switch
    {
        AssignmentStatus.Completed => "Tamamlandı",
        AssignmentStatus.Partial => "Devam Ediyor",
        AssignmentStatus.NotStarted => "Devam Ediyor",
        AssignmentStatus.Disqualify => "Diskalifiye",
        AssignmentStatus.QuotaFull => "Kota Dolu",
        AssignmentStatus.ScreenOut => "Elenmiş",
        _ => "Bilinmiyor"
    };
}
