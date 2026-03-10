using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class ExternalAffiliateServiceTests
{
    private readonly Mock<IExternalAffiliateRepository> _affiliateRepoMock = new();
    private readonly Mock<ISubjectRepository> _subjectRepoMock = new();
    private readonly Mock<ILogger<ExternalAffiliateService>> _loggerMock = new();
    private readonly ExternalAffiliateService _sut;

    public ExternalAffiliateServiceTests()
    {
        _sut = new ExternalAffiliateService(
            _affiliateRepoMock.Object,
            _subjectRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_WhenDuplicateCode_ThrowsBusinessException()
    {
        _affiliateRepoMock.Setup(x => x.ExistsByCodeAsync("TEST", 0)).ReturnsAsync(true);

        var act = () => _sut.CreateOrUpdateAsync(
            0, "Test", "test@example.com", null, null, "TEST", null, true, null, 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*affiliate kodu*");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_WhenCodeConflictsWithSubjectReferral_ThrowsBusinessException()
    {
        _affiliateRepoMock.Setup(x => x.ExistsByCodeAsync("ABC123", 0)).ReturnsAsync(false);
        _affiliateRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), 0)).ReturnsAsync(false);
        _subjectRepoMock.Setup(x => x.GetByReferralCodeAsync("ABC123"))
            .ReturnsAsync(new Subject { Id = 99, ReferralCode = "ABC123" });

        var act = () => _sut.CreateOrUpdateAsync(
            0, "Test", "test@example.com", null, null, "ABC123", null, true, null, 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*denek referral kodu*");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_WhenNew_CreatesAffiliate()
    {
        _affiliateRepoMock.Setup(x => x.ExistsByCodeAsync(It.IsAny<string>(), 0)).ReturnsAsync(false);
        _affiliateRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), 0)).ReturnsAsync(false);
        _subjectRepoMock.Setup(x => x.GetByReferralCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Subject?)null);

        await _sut.CreateOrUpdateAsync(
            0, "Test Partner", "partner@example.com", "+905551234567", "TR120001234567890",
            "PARTNER1", 15m, true, "Test notu", 1);

        _affiliateRepoMock.Verify(x => x.AddAsync(It.Is<ExternalAffiliate>(a =>
            a.Name == "Test Partner"
            && a.AffiliateCode == "PARTNER1"
            && a.CommissionAmount == 15m)),
            Times.Once);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenInsufficientBalance_ThrowsBusinessException()
    {
        var affiliate = new ExternalAffiliate { Id = 1, Balance = 50m };
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(affiliate);

        var act = () => _sut.RecordPaymentAsync(1, 100m, "Ödeme", 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Yetersiz bakiye*");
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenValid_UpdatesBalanceAndCreatesTransaction()
    {
        var affiliate = new ExternalAffiliate { Id = 1, Balance = 200m, TotalPaid = 50m };
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(affiliate);

        await _sut.RecordPaymentAsync(1, 100m, "Mart ödemesi", 1);

        affiliate.Balance.Should().Be(100m);
        affiliate.TotalPaid.Should().Be(150m);

        _affiliateRepoMock.Verify(
            x => x.RecordPaymentTransactionAsync(
                It.Is<ExternalAffiliate>(a => a.Balance == 100m),
                It.Is<AffiliateTransaction>(t =>
                    t.Type == AffiliateTransactionType.Payment
                    && t.Amount == 100m)),
            Times.Once);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenZeroOrNegativeAmount_ThrowsBusinessException()
    {
        var act = () => _sut.RecordPaymentAsync(1, -5m, "Ödeme", 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*sıfırdan büyük*");
    }

    [Fact]
    public async Task RecordMovementAsync_CreditIncreasesBalanceAndTotalEarned()
    {
        var affiliate = new ExternalAffiliate { Id = 1, Balance = 100m, TotalEarned = 200m };
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(affiliate);

        await _sut.RecordMovementAsync(1, 50m, true, "Bonus", 1);

        affiliate.Balance.Should().Be(150m);
        affiliate.TotalEarned.Should().Be(250m);

        _affiliateRepoMock.Verify(
            x => x.RecordAdjustmentTransactionAsync(
                It.IsAny<ExternalAffiliate>(),
                It.Is<AffiliateTransaction>(t =>
                    t.Type == AffiliateTransactionType.Credit
                    && t.Amount == 50m)),
            Times.Once);
    }

    [Fact]
    public async Task RecordMovementAsync_DebitDecreasesBalance()
    {
        var affiliate = new ExternalAffiliate { Id = 1, Balance = 100m, TotalEarned = 200m };
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(affiliate);

        await _sut.RecordMovementAsync(1, 30m, false, "Kesinti", 1);

        affiliate.Balance.Should().Be(70m);
        affiliate.TotalEarned.Should().Be(200m); // Eksi harekette TotalEarned değişmez

        _affiliateRepoMock.Verify(
            x => x.RecordAdjustmentTransactionAsync(
                It.IsAny<ExternalAffiliate>(),
                It.Is<AffiliateTransaction>(t =>
                    t.Type == AffiliateTransactionType.Debit
                    && t.Amount == 30m)),
            Times.Once);
    }

    [Fact]
    public async Task RecordMovementAsync_DebitInsufficientBalance_ThrowsBusinessException()
    {
        var affiliate = new ExternalAffiliate { Id = 1, Balance = 20m };
        _affiliateRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(affiliate);

        var act = () => _sut.RecordMovementAsync(1, 50m, false, "Kesinti", 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*bakiye yetersiz*");
    }

    [Fact]
    public async Task RecordMovementAsync_ZeroAmount_ThrowsBusinessException()
    {
        var act = () => _sut.RecordMovementAsync(1, 0m, true, "Test", 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*sıfırdan büyük*");
    }
}
