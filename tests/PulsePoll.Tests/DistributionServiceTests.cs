using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class DistributionServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepository = new();
    private readonly Mock<IDistributionLogRepository> _distributionLogRepository = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<ILogger<DistributionService>> _logger = new();
    private readonly DistributionService _sut;

    public DistributionServiceTests()
    {
        _sut = new DistributionService(
            _projectRepository.Object,
            _distributionLogRepository.Object,
            _notificationService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task RunHourlyDistributionAsync_WhenProjectIsNotActive_ThrowsBusinessException()
    {
        var project = CreateScheduledProject();
        project.Status = ProjectStatus.Paused;

        _projectRepository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);

        var act = async () => await _sut.RunHourlyDistributionAsync(project.Id);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be("PROJECT_NOT_ACTIVE");
    }

    [Fact]
    public async Task SendReminderNotificationsAsync_WhenReminderSent_UpdatesNotificationTimestamp()
    {
        var project = CreateScheduledProject();
        var assignments = new List<ProjectAssignment>
        {
            new() { Id = 11, ProjectId = project.Id, SubjectId = 101, Status = AssignmentStatus.NotStarted, ScheduledNotifiedAt = new DateTime(2026, 3, 7, 9, 0, 0) },
            new() { Id = 12, ProjectId = project.Id, SubjectId = 102, Status = AssignmentStatus.NotStarted, ScheduledNotifiedAt = new DateTime(2026, 3, 7, 10, 0, 0) }
        };

        _projectRepository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);
        _projectRepository.Setup(x => x.GetNotStartedNeedingReminderAsync(project.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(assignments);

        var count = await _sut.SendReminderNotificationsAsync(project.Id);

        count.Should().Be(2);
        _notificationService.Verify(
            x => x.SendPushToManyAsync(
                It.Is<IEnumerable<int>>(ids => ids.OrderBy(id => id).SequenceEqual(new[] { 101, 102 })),
                "Anketin seni bekliyor!",
                project.StartMessage,
                "survey_reminder",
                null),
            Times.Once);
        _projectRepository.Verify(
            x => x.UpdateAssignmentsStatusBatchAsync(
                It.Is<IEnumerable<int>>(ids => ids.OrderBy(id => id).SequenceEqual(new[] { 11, 12 })),
                AssignmentStatus.NotStarted,
                It.IsAny<DateTime?>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDistributionProgressAsync_UsesAggregatedCounts()
    {
        var project = CreateScheduledProject();
        var counts = new List<AssignmentStatusCountDto>
        {
            new(AssignmentStatus.Scheduled, 6),
            new(AssignmentStatus.NotStarted, 3),
            new(AssignmentStatus.Completed, 2),
            new(AssignmentStatus.Disqualify, 1)
        };

        _projectRepository.Setup(x => x.GetByIdAsync(project.Id)).ReturnsAsync(project);
        _projectRepository.Setup(x => x.GetAssignmentStatusCountsAsync(project.Id)).ReturnsAsync(counts);
        _distributionLogRepository.Setup(x => x.GetTodayDistributedCountAsync(project.Id, It.IsAny<DateOnly>())).ReturnsAsync(2);

        var result = await _sut.GetDistributionProgressAsync(project.Id);

        result.TotalAssigned.Should().Be(12);
        result.ScheduledCount.Should().Be(6);
        result.NotStartedCount.Should().Be(3);
        result.CompletedCount.Should().Be(2);
        result.OtherStatusCount.Should().Be(1);
        _projectRepository.Verify(x => x.GetAssignmentStatusCountsAsync(project.Id), Times.Once);
        _projectRepository.Verify(x => x.GetAssignmentsByProjectAsync(It.IsAny<int>()), Times.Never);
    }

    private static Project CreateScheduledProject()
        => new()
        {
            Id = 7,
            Name = "Dağıtım Projesi",
            Status = ProjectStatus.Active,
            IsScheduledDistribution = true,
            DistributionStartHour = new TimeOnly(9, 0),
            DistributionEndHour = new TimeOnly(19, 0),
            StartMessage = "Başlangıç",
            StartDate = new DateOnly(2026, 3, 1),
            DurationDays = 10
        };
}
