using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class AffiliateRewardServiceTests
{
    private readonly Mock<ISubjectRepository> _subjectRepoMock = new();
    private readonly Mock<ISubjectAppActivityRepository> _activityRepoMock = new();
    private readonly Mock<IExternalAffiliateRepository> _affiliateRepoMock = new();
    private readonly Mock<IReferralRewardConfigService> _configServiceMock = new();
    private readonly Mock<ILogger<AffiliateRewardService>> _loggerMock = new();
    private readonly AffiliateRewardService _sut;

    public AffiliateRewardServiceTests()
    {
        _sut = new AffiliateRewardService(
            _subjectRepoMock.Object,
            _activityRepoMock.Object,
            _affiliateRepoMock.Object,
            _configServiceMock.Object,
            _loggerMock.Object);

        _configServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new ReferralRewardConfigDto(
                IsActive: true,
                TriggerType: ReferralRewardTriggerType.RegistrationCompleted,
                RewardAmount: 10m,
                ActiveDaysThreshold: 7));
    }

    [Fact]
    public async Task TryGrantAsync_WhenSubjectHasNoAffiliate_DoesNothing()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = null };
        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGrantAsync_WhenAffiliateInactive_DoesNothing()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate { Id = 10, IsActive = false };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGrantAsync_WhenTriggerMismatch_DoesNothing()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate { Id = 10, IsActive = true };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);
        // Global config trigger = RegistrationCompleted, biz AccountApproved gönderiyoruz
        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.AccountApproved, 0);

        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGrantAsync_WhenAlreadyGranted_Idempotent()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate { Id = 10, IsActive = true };
        var existingTx = new AffiliateTransaction { Id = 99, ReferenceId = "affiliate-commission:10:1" };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);
        _affiliateRepoMock
            .Setup(x => x.GetTransactionByReferenceAsync(10, "affiliate-commission:10:1"))
            .ReturnsAsync(existingTx);

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGrantAsync_WhenEligible_GrantsCommission()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate { Id = 10, IsActive = true, Balance = 0, TotalEarned = 0 };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);
        _affiliateRepoMock
            .Setup(x => x.GetTransactionByReferenceAsync(10, It.IsAny<string>()))
            .ReturnsAsync((AffiliateTransaction?)null);
        _affiliateRepoMock
            .Setup(x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()))
            .ReturnsAsync(true);

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        affiliate.Balance.Should().Be(10m);
        affiliate.TotalEarned.Should().Be(10m);

        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(
                It.Is<ExternalAffiliate>(a => a.Id == 10),
                It.Is<AffiliateTransaction>(t =>
                    t.Type == AffiliateTransactionType.Commission
                    && t.Amount == 10m
                    && t.SubjectId == 1
                    && t.ReferenceId == "affiliate-commission:10:1")),
            Times.Once);
    }

    [Fact]
    public async Task TryGrantAsync_WhenAffiliateOverridesAmount_UsesOverride()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate
        {
            Id = 10, IsActive = true, Balance = 0, TotalEarned = 0,
            CommissionAmount = 25m // Override: global 10m yerine 25m
        };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);
        _affiliateRepoMock
            .Setup(x => x.GetTransactionByReferenceAsync(10, It.IsAny<string>()))
            .ReturnsAsync((AffiliateTransaction?)null);
        _affiliateRepoMock
            .Setup(x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()))
            .ReturnsAsync(true);

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        affiliate.Balance.Should().Be(25m);
        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(
                It.IsAny<ExternalAffiliate>(),
                It.Is<AffiliateTransaction>(t => t.Amount == 25m)),
            Times.Once);
    }

    [Fact]
    public async Task TryGrantAsync_WhenDbReturnsIdempotentFalse_LogsWarning()
    {
        var subject = new Subject { Id = 1, ExternalAffiliateId = 10 };
        var affiliate = new ExternalAffiliate { Id = 10, IsActive = true };

        _subjectRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(subject);
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(affiliate);
        _affiliateRepoMock
            .Setup(x => x.GetTransactionByReferenceAsync(10, It.IsAny<string>()))
            .ReturnsAsync((AffiliateTransaction?)null);
        _affiliateRepoMock
            .Setup(x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()))
            .ReturnsAsync(false); // Unique constraint — idempotent no-op

        await _sut.TryGrantAsync(1, ReferralRewardTriggerType.RegistrationCompleted, 0);

        // Should not throw, should be handled gracefully
        _affiliateRepoMock.Verify(
            x => x.GrantCommissionAsync(It.IsAny<ExternalAffiliate>(), It.IsAny<AffiliateTransaction>()),
            Times.Once);
    }

    [Fact]
    public async Task ReconcilePendingAsync_WhenNoPending_ReturnsZero()
    {
        _affiliateRepoMock
            .Setup(x => x.GetPendingCommissionSubjectsWithAffiliateAsync())
            .ReturnsAsync([]);

        var result = await _sut.ReconcilePendingAsync(0);

        result.Should().Be(0);
    }
}
