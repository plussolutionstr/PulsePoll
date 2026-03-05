using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using Xunit;

namespace PulsePoll.Tests;

public class SubjectServiceTests
{
    private readonly Mock<ISubjectRepository> _repoMock = new();
    private readonly Mock<ISubjectScoreService> _scoreServiceMock = new();
    private readonly Mock<IReferralRewardService> _referralRewardServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IMediaUrlService> _mediaUrlServiceMock = new();
    private readonly Mock<IMessagePublisher> _publisherMock = new();
    private readonly Mock<ILogger<SubjectService>> _loggerMock = new();
    private readonly SubjectService _sut;

    public SubjectServiceTests()
    {
        _sut = new SubjectService(
            _repoMock.Object,
            _scoreServiceMock.Object,
            _referralRewardServiceMock.Object,
            _storageServiceMock.Object,
            _mediaUrlServiceMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubjectExists_ReturnsDto()
    {
        // TODO: implement
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubjectNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Subject?)null);

        var result = await _sut.GetByIdAsync(99);

        result.Should().BeNull();
    }
}
