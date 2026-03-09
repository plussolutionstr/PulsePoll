using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Application.Validators;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _repository = new();
    private readonly Mock<IMediaUrlService> _mediaUrlService = new();
    private readonly Mock<IRewardUnitConfigService> _rewardUnitConfigService = new();
    private readonly IValidator<CreateProjectDto> _createValidator = new CreateProjectValidator();
    private readonly IValidator<UpdateProjectDto> _updateValidator = new UpdateProjectValidator();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _rewardUnitConfigService
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new RewardUnitConfigDto("TRY", "TL", 1m));

        _sut = new ProjectService(
            _repository.Object,
            _mediaUrlService.Object,
            _rewardUnitConfigService.Object,
            _createValidator,
            _updateValidator);
    }

    [Fact]
    public async Task UpdateAndDisableScheduledDistributionAsync_ConvertsScheduledAssignmentsBeforeSingleSave()
    {
        var project = new Project
        {
            Id = 10,
            CustomerId = 1,
            Code = "PRJ10",
            Name = "Eski Proje",
            Status = ProjectStatus.Active,
            IsScheduledDistribution = true,
            DistributionStartHour = new TimeOnly(9, 0),
            DistributionEndHour = new TimeOnly(19, 0),
            SurveyUrl = "https://example.com",
            SubjectParameterName = "uid",
            ProjectParameterName = "pid",
            StartMessage = "Başlangıç",
            CompletedMessage = "Bitti",
            DisqualifyMessage = "Diskalifiye",
            QuotaFullMessage = "Kota dolu",
            ScreenOutMessage = "Screen out",
            DurationDays = 5,
            ParticipantCount = 100,
            TotalTargetCount = 50,
            EstimatedMinutes = 10
        };

        var assignments = new List<ProjectAssignment>
        {
            new() { Id = 1, ProjectId = 10, SubjectId = 101, Status = AssignmentStatus.Scheduled },
            new() { Id = 2, ProjectId = 10, SubjectId = 102, Status = AssignmentStatus.Scheduled }
        };

        _repository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);
        _repository.Setup(x => x.GetScheduledAssignmentsAsync(project.Id, int.MaxValue)).ReturnsAsync(assignments);

        var dto = new UpdateProjectDto(
            Name: "Yeni Proje",
            Description: "Açıklama",
            Category: "Kategori",
            ParticipantCount: 100,
            TotalTargetCount: 50,
            DurationDays: 5,
            StartDate: new DateOnly(2026, 3, 8),
            Budget: 1000m,
            Reward: 10m,
            ConsolationReward: 2m,
            SurveyUrl: "https://example.com",
            SubjectParameterName: "uid",
            ProjectParameterName: "pid",
            EstimatedMinutes: 10,
            CustomerBriefing: "Brief",
            StartMessage: "Başlangıç",
            CompletedMessage: "Bitti",
            DisqualifyMessage: "Diskalifiye",
            QuotaFullMessage: "Kota dolu",
            ScreenOutMessage: "Screen out",
            Status: ProjectStatus.Active,
            CoverMediaId: null,
            IsScheduledDistribution: false,
            DistributionStartHour: new TimeOnly(9, 0),
            DistributionEndHour: new TimeOnly(19, 0));

        var converted = await _sut.UpdateAndDisableScheduledDistributionAsync(project.Id, dto, adminId: 5);

        converted.Should().Be(2);
        project.IsScheduledDistribution.Should().BeFalse();
        assignments.Should().OnlyContain(x => x.Status == AssignmentStatus.NotStarted);
        _repository.Verify(x => x.UpdateAsync(project), Times.Once);
        _repository.Verify(x => x.UpdateAssignmentsStatusBatchAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<AssignmentStatus>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task FindSurveyHelperMatchAsync_ReturnsLongestContainsMatch()
    {
        var project = new Project
        {
            Id = 21,
            CustomerId = 1,
            Code = "PRJ21",
            Name = "Helper Proje",
            SupportsSurveyHelper = true
        };

        var entries = new List<ProjectSurveyHelperEntry>
        {
            new() { Id = 1, ProjectId = 21, QuestionText = "hazır giyim", HelpText = "Kısa eşleşme" },
            new() { Id = 2, ProjectId = 21, QuestionText = "En sık alışveriş yaptığınız hazır giyim markaları nelerdir?", HelpText = "Tam eşleşme" }
        };

        _repository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);
        _repository.Setup(x => x.GetSurveyHelperEntriesAsync(project.Id)).ReturnsAsync(entries);

        var result = await _sut.FindSurveyHelperMatchAsync(project.Id, "En sık alışveriş yaptığınız hazır giyim markaları nelerdir?");

        result.Found.Should().BeTrue();
        result.Message.Should().Be("Tam eşleşme");
    }

    [Fact]
    public async Task FindSurveyHelperMatchAsync_ReturnsDisabledMessage_WhenProjectDoesNotSupportHelper()
    {
        var project = new Project
        {
            Id = 22,
            CustomerId = 1,
            Code = "PRJ22",
            Name = "Helper Kapalı",
            SupportsSurveyHelper = false
        };

        _repository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);

        var result = await _sut.FindSurveyHelperMatchAsync(project.Id, "örnek soru");

        result.Found.Should().BeFalse();
        result.Message.Should().Be("Bu proje için survey helper aktif değil.");
    }

    [Fact]
    public async Task FindSurveyHelperMatchAsync_ReturnsNotFound_WhenNoEntryMatches()
    {
        var project = new Project
        {
            Id = 23,
            CustomerId = 1,
            Code = "PRJ23",
            Name = "Helper Proje",
            SupportsSurveyHelper = true
        };

        var entries = new List<ProjectSurveyHelperEntry>
        {
            new() { Id = 1, ProjectId = 23, QuestionText = "farklı soru", HelpText = "Yardım" }
        };

        _repository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);
        _repository.Setup(x => x.GetSurveyHelperEntriesAsync(project.Id)).ReturnsAsync(entries);

        var result = await _sut.FindSurveyHelperMatchAsync(project.Id, "eşleşmeyen metin");

        result.Found.Should().BeFalse();
        result.Message.Should().Be("Bu soru için yardım bulunamadı.");
    }
}
