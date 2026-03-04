using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.ApiModels;

public static class ApiDtoMapper
{
    public static StoryModel ToModel(this StoryApiDto dto)
        => new(dto.Id, dto.BrandName ?? dto.Title, dto.ImageUrl);

    public static NewsModel ToModel(this NewsApiDto dto)
        => new(dto.Id, "", dto.Title, dto.Summary);

    public static SurveyModel ToModel(this ProjectApiDto dto)
        => new(
            dto.Id,
            dto.CustomerShortName,
            dto.CoverImageUrl ?? "",
            dto.Name,
            dto.Description ?? "",
            "",
            dto.Reward,
            dto.EstimatedMinutes,
            0,
            []);
}
