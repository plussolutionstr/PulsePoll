using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
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
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
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
            _cacheMock.Object,
            _refreshTokenRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubjectExists_ReturnsDto()
    {
        var subject = new Subject
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Ali",
            LastName = "Yılmaz",
            PhoneNumber = "5551234567",
            Gender = Gender.Male,
            BirthDate = new DateOnly(1990, 1, 1),
            MaritalStatus = MaritalStatus.Single,
            GsmOperator = GsmOperator.Turkcell,
            ReferralCode = "ABC123",
            Status = ApprovalStatus.Approved
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _scoreServiceMock.Setup(s => s.GetCurrentAsync(1)).ReturnsAsync((Application.DTOs.SubjectScoreDto?)null);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.FirstName.Should().Be("Ali");
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubjectNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Subject?)null);

        var result = await _sut.GetByIdAsync(99);

        result.Should().BeNull();
    }
}
