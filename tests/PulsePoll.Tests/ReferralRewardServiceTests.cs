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

public class ReferralRewardServiceTests
{
    private readonly Mock<ISubjectRepository> _subjectRepositoryMock = new();
    private readonly Mock<ISubjectAppActivityRepository> _activityRepositoryMock = new();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IReferralRewardConfigService> _referralRewardConfigServiceMock = new();
    private readonly Mock<IRewardUnitConfigService> _rewardUnitConfigServiceMock = new();
    private readonly Mock<ILogger<ReferralRewardService>> _loggerMock = new();

    [Fact]
    public async Task ReconcilePendingAsync_WhenApprovedReferralExists_GrantsMissingReward()
    {
        var referral = new Referral
        {
            Id = 10,
            ReferrerId = 7,
            ReferredSubjectId = 42,
            ReferredAt = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            ReferredSubject = new Subject
            {
                Id = 42,
                Status = ApprovalStatus.Approved
            }
        };

        var wallet = new Wallet
        {
            Id = 4,
            SubjectId = 7,
            Balance = 0m,
            TotalEarned = 0m
        };

        _subjectRepositoryMock
            .Setup(x => x.GetPendingRewardReferralsAsync())
            .ReturnsAsync([referral]);
        _subjectRepositoryMock
            .Setup(x => x.GetReferralByReferredSubjectIdAsync(42))
            .ReturnsAsync(referral);

        _referralRewardConfigServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new ReferralRewardConfigDto(
                IsActive: true,
                RewardAmount: 25m,
                TriggerType: ReferralRewardTriggerType.AccountApproved,
                ActiveDaysThreshold: 7));

        _rewardUnitConfigServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new RewardUnitConfigDto("TRY", "TL", 1m));

        _walletRepositoryMock
            .Setup(x => x.GetBySubjectIdAsync(7))
            .ReturnsAsync(wallet);
        _walletRepositoryMock
            .Setup(x => x.GetTransactionByReferenceAsync(4, "referral-reward:10"))
            .ReturnsAsync((WalletTransaction?)null);
        _walletRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Wallet>()))
            .Returns(Task.CompletedTask);
        _walletRepositoryMock
            .Setup(x => x.AddTransactionAsync(It.IsAny<WalletTransaction>()))
            .Returns(Task.CompletedTask);
        _subjectRepositoryMock
            .Setup(x => x.UpdateReferralAsync(It.IsAny<Referral>()))
            .Returns(Task.CompletedTask);

        var sut = new ReferralRewardService(
            _subjectRepositoryMock.Object,
            _activityRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _referralRewardConfigServiceMock.Object,
            _rewardUnitConfigServiceMock.Object,
            _loggerMock.Object);

        var result = await sut.ReconcilePendingAsync(actorId: 0);

        result.Should().Be(1);
        wallet.Balance.Should().Be(25m);
        wallet.TotalEarned.Should().Be(25m);
        referral.CommissionEarned.Should().Be(25m);

        _walletRepositoryMock.Verify(
            x => x.AddTransactionAsync(It.Is<WalletTransaction>(tx =>
                tx.WalletId == 4 &&
                tx.Amount == 25m &&
                tx.ReferenceId == "referral-reward:10")),
            Times.Once);
        _subjectRepositoryMock.Verify(
            x => x.UpdateReferralAsync(It.Is<Referral>(r => r.Id == 10 && r.CommissionEarned == 25m)),
            Times.Once);
    }
}
