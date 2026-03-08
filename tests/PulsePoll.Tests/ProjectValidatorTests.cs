using FluentAssertions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Validators;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class ProjectValidatorTests
{
    [Fact]
    public void CreateValidator_WhenScheduledDistributionHasInvalidWindow_ReturnsValidationErrors()
    {
        var validator = new CreateProjectValidator();
        var dto = CreateValidCreateDto() with
        {
            IsScheduledDistribution = true,
            DistributionStartHour = new TimeOnly(19, 0),
            DistributionEndHour = new TimeOnly(18, 0)
        };

        var result = validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("başlangıç saati bitiş saatinden önce"));
    }

    [Fact]
    public void UpdateValidator_WhenScheduledDistributionUsesHalfHour_ReturnsValidationErrors()
    {
        var validator = new UpdateProjectValidator();
        var dto = CreateValidUpdateDto() with
        {
            IsScheduledDistribution = true,
            DistributionStartHour = new TimeOnly(9, 30),
            DistributionEndHour = new TimeOnly(18, 0)
        };

        var result = validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("tam saat"));
    }

    private static CreateProjectDto CreateValidCreateDto()
        => new(
            CustomerId: 1,
            Code: "PRJ001",
            Name: "Proje",
            Description: "Açıklama",
            Category: "Araştırma",
            ParticipantCount: 100,
            TotalTargetCount: 50,
            DurationDays: 5,
            StartDate: new DateOnly(2026, 3, 8),
            Budget: 1000,
            Reward: 10,
            ConsolationReward: 2,
            SurveyUrl: "https://example.com/survey",
            SubjectParameterName: "uid",
            ProjectParameterName: "pid",
            EstimatedMinutes: 10,
            CustomerBriefing: "Brief",
            StartMessage: "Merhaba",
            CompletedMessage: "Teşekkürler",
            DisqualifyMessage: "Üzgünüz",
            QuotaFullMessage: "Kota dolu",
            ScreenOutMessage: "Uygun değilsiniz",
            CoverMediaId: null,
            IsScheduledDistribution: false,
            DistributionStartHour: default,
            DistributionEndHour: default);

    private static UpdateProjectDto CreateValidUpdateDto()
        => new(
            Name: "Proje",
            Description: "Açıklama",
            Category: "Araştırma",
            ParticipantCount: 100,
            TotalTargetCount: 50,
            DurationDays: 5,
            StartDate: new DateOnly(2026, 3, 8),
            Budget: 1000,
            Reward: 10,
            ConsolationReward: 2,
            SurveyUrl: "https://example.com/survey",
            SubjectParameterName: "uid",
            ProjectParameterName: "pid",
            EstimatedMinutes: 10,
            CustomerBriefing: "Brief",
            StartMessage: "Merhaba",
            CompletedMessage: "Teşekkürler",
            DisqualifyMessage: "Üzgünüz",
            QuotaFullMessage: "Kota dolu",
            ScreenOutMessage: "Uygun değilsiniz",
            Status: ProjectStatus.Active,
            CoverMediaId: null,
            IsScheduledDistribution: false,
            DistributionStartHour: default,
            DistributionEndHour: default);
}
