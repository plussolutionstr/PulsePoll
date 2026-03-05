using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Xunit;

namespace PulsePoll.Tests;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository>                _walletRepoMock              = new();
    private readonly Mock<IProjectRepository>               _projectRepoMock             = new();
    private readonly Mock<IWithdrawalRequestRepository>    _withdrawalRequestRepoMock   = new();
    private readonly Mock<IPaymentSettingRepository>        _paymentSettingRepoMock      = new();
    private readonly Mock<INotificationRepository>          _notificationRepoMock        = new();
    private readonly Mock<ISubjectRepository>               _subjectRepoMock             = new();
    private readonly Mock<ILookupService>                   _lookupServiceMock           = new();
    private readonly Mock<IMediaUrlService>                 _mediaUrlServiceMock         = new();
    private readonly Mock<IRewardUnitConfigService>         _rewardUnitConfigServiceMock = new();
    private readonly Mock<IMessagePublisher>                _publisherMock               = new();
    private readonly Mock<IValidator<WithdrawalRequestDto>> _withdrawalValidator         = new();
    private readonly Mock<IValidator<AddBankAccountDto>>    _bankAccountValidator        = new();
    private readonly Mock<ILogger<WalletService>>           _loggerMock                  = new();
    private readonly WalletService _sut;

    public WalletServiceTests()
    {
        _sut = new WalletService(
            _walletRepoMock.Object,
            _projectRepoMock.Object,
            _withdrawalRequestRepoMock.Object,
            _paymentSettingRepoMock.Object,
            _notificationRepoMock.Object,
            _subjectRepoMock.Object,
            _lookupServiceMock.Object,
            _mediaUrlServiceMock.Object,
            _rewardUnitConfigServiceMock.Object,
            _publisherMock.Object,
            _withdrawalValidator.Object,
            _bankAccountValidator.Object,
            _loggerMock.Object);

        // Validatörler varsayılan olarak geçer
        _withdrawalValidator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _bankAccountValidator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _rewardUnitConfigServiceMock
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new RewardUnitConfigDto("POLL", "Poll", 1m));

        _projectRepoMock
            .Setup(r => r.GetSubjectAssignmentsAsync(It.IsAny<int>()))
            .ReturnsAsync([]);
    }

    // ────────────────── Helpers ──────────────────

    private static Wallet MakeWallet(int subjectId = 1, decimal balance = 100m)
    {
        var w = new Wallet { Id = 10, SubjectId = subjectId, Balance = balance, TotalEarned = balance };
        w.SetCreated(subjectId);
        return w;
    }

    private static BankAccount MakeBankAccount(int subjectId = 1, int id = 5)
        => new() { Id = id, SubjectId = subjectId, BankName = "Test Bank", IbanLast4 = "1234", IsDefault = true };

    private static Subject MakeSubject(int id = 1, string? fcmToken = null)
        => new() { Id = id, FcmToken = fcmToken };

    // ────────────────── CreditAsync ──────────────────

    [Fact]
    public async Task CreditAsync_UpdatesBalanceAndTotalEarned()
    {
        var wallet = MakeWallet(balance: 50m);
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(wallet);
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeSubject());

        await _sut.CreditAsync(subjectId: 1, amount: 25m, referenceId: "ref-1", description: "Test");

        wallet.Balance.Should().Be(75m);
        wallet.TotalEarned.Should().Be(75m);
    }

    [Fact]
    public async Task CreditAsync_PersistsTransactionWithCorrectType()
    {
        var wallet = MakeWallet();
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(wallet);
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeSubject());

        await _sut.CreditAsync(1, 30m, "ref-2", "Anket ödülü");

        _walletRepoMock.Verify(
            r => r.AddTransactionAsync(It.Is<WalletTransaction>(t =>
                t.Amount == 30m &&
                t.Type == WalletTransactionType.Credit &&
                t.ReferenceId == "ref-2")),
            Times.Once);
    }

    [Fact]
    public async Task CreditAsync_PublishesNotification_WhenSubjectHasFcmToken()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(MakeWallet());
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeSubject(fcmToken: "fcm-token-xyz"));

        await _sut.CreditAsync(1, 20m, "ref-3", "Ödül");

        _publisherMock.Verify(
            p => p.PublishAsync(
                It.Is<NotificationSendMessage>(m => m.SubjectId == 1 && m.FcmToken == "fcm-token-xyz"),
                Queues.NotificationSend,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreditAsync_DoesNotPublishNotification_WhenSubjectHasNoFcmToken()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(MakeWallet());
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeSubject(fcmToken: null));

        await _sut.CreditAsync(1, 20m, "ref-4", "Ödül");

        _publisherMock.Verify(
            p => p.PublishAsync(
                It.IsAny<NotificationSendMessage>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreditAsync_ThrowsNotFoundException_WhenWalletNotFound()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(It.IsAny<int>())).ReturnsAsync((Wallet?)null);

        await _sut.Invoking(s => s.CreditAsync(1, 10m, "ref", "desc"))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ────────────────── RequestWithdrawalAsync ──────────────────

    [Fact]
    public async Task RequestWithdrawalAsync_CallsCreateWithdrawalTransactionAsync()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 5)).ReturnsAsync(MakeBankAccount());
        _walletRepoMock
            .Setup(r => r.CreateWithdrawalTransactionAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1); // transaction ID

        await _sut.RequestWithdrawalAsync(subjectId: 1, new WithdrawalRequestDto(Amount: 100m, BankAccountId: 5));

        _walletRepoMock.Verify(
            r => r.CreateWithdrawalTransactionAsync(
                1, 5, 100m, It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestWithdrawalAsync_ThrowsNotFoundException_WhenBankAccountNotFoundDuringRequest()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 5)).ReturnsAsync((BankAccount?)null);

        await _sut.Invoking(s => s.RequestWithdrawalAsync(1, new WithdrawalRequestDto(100m, 5)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RequestWithdrawalAsync_ValidatesInputUsingValidator()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 5)).ReturnsAsync(MakeBankAccount());
        _walletRepoMock
            .Setup(r => r.CreateWithdrawalTransactionAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        await _sut.RequestWithdrawalAsync(1, new WithdrawalRequestDto(50m, 5));

        _withdrawalValidator.Verify(
            v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestWithdrawalAsync_ThrowsNotFoundException_WhenWalletNotFound()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(It.IsAny<int>())).ReturnsAsync((Wallet?)null);

        await _sut.Invoking(s => s.RequestWithdrawalAsync(1, new WithdrawalRequestDto(50m, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RequestWithdrawalAsync_ThrowsNotFoundException_WhenBankAccountNotFound()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(MakeWallet(balance: 200m));
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 99)).ReturnsAsync((BankAccount?)null);

        await _sut.Invoking(s => s.RequestWithdrawalAsync(1, new WithdrawalRequestDto(50m, 99)))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ────────────────── GetBySubjectIdAsync ──────────────────

    [Fact]
    public async Task GetBySubjectIdAsync_ReturnsCorrectDto()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(7)).ReturnsAsync(MakeWallet(subjectId: 7, balance: 250m));

        var result = await _sut.GetBySubjectIdAsync(7);

        result.SubjectId.Should().Be(7);
        result.Balance.Should().Be(250m);
        result.TotalEarned.Should().Be(250m);
    }

    [Fact]
    public async Task GetBySubjectIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(It.IsAny<int>())).ReturnsAsync((Wallet?)null);

        await _sut.Invoking(s => s.GetBySubjectIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ────────────────── GetTransactionsAsync ──────────────────

    [Fact]
    public async Task GetTransactionsAsync_ReturnsPagedResult_WithCorrectCounts()
    {
        var wallet = MakeWallet();
        var transactions = new List<WalletTransaction>
        {
            new() { Id = 1, WalletId = 10, Amount = 10m, Type = WalletTransactionType.Credit },
            new() { Id = 2, WalletId = 10, Amount = 5m,  Type = WalletTransactionType.Withdrawal }
        };

        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(wallet);
        _walletRepoMock.Setup(r => r.CountTransactionsAsync(10)).ReturnsAsync(2);
        _walletRepoMock.Setup(r => r.GetTransactionsAsync(10, 0, 20)).ReturnsAsync(transactions);

        var result = await _sut.GetTransactionsAsync(subjectId: 1, page: 1, pageSize: 20);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactionsAsync_CalculatesSkipCorrectly()
    {
        var wallet = MakeWallet();
        _walletRepoMock.Setup(r => r.GetBySubjectIdAsync(1)).ReturnsAsync(wallet);
        _walletRepoMock.Setup(r => r.CountTransactionsAsync(10)).ReturnsAsync(50);
        _walletRepoMock.Setup(r => r.GetTransactionsAsync(10, 20, 10)).ReturnsAsync([]);

        await _sut.GetTransactionsAsync(subjectId: 1, page: 3, pageSize: 10);

        // page=3, pageSize=10 → skip=20
        _walletRepoMock.Verify(r => r.GetTransactionsAsync(10, 20, 10), Times.Once);
    }

    // ────────────────── AddBankAccountAsync ──────────────────

    [Fact]
    public async Task AddBankAccountAsync_SetsIsDefault_WhenFirstAccount()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountsAsync(1)).ReturnsAsync([]);

        BankAccount? captured = null;
        _walletRepoMock
            .Setup(r => r.AddBankAccountAsync(It.IsAny<BankAccount>()))
            .Callback<BankAccount>(a => captured = a)
            .Returns(Task.CompletedTask);

        _lookupServiceMock.Setup(l => l.GetBankByIdAsync(1)).ReturnsAsync(new Domain.Entities.Bank { Id = 1, Name = "Test Bank", IsActive = true });
        await _sut.AddBankAccountAsync(subjectId: 1, new AddBankAccountDto(1, "TR123456789012345678901234"));

        captured.Should().NotBeNull();
        captured!.IsDefault.Should().BeTrue();
        captured.IbanLast4.Should().Be("1234");
    }

    [Fact]
    public async Task AddBankAccountAsync_DoesNotSetIsDefault_WhenSecondAccount()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountsAsync(1)).ReturnsAsync([MakeBankAccount()]);

        BankAccount? captured = null;
        _walletRepoMock
            .Setup(r => r.AddBankAccountAsync(It.IsAny<BankAccount>()))
            .Callback<BankAccount>(a => captured = a)
            .Returns(Task.CompletedTask);

        _lookupServiceMock.Setup(l => l.GetBankByIdAsync(2)).ReturnsAsync(new Domain.Entities.Bank { Id = 2, Name = "Another Bank", IsActive = true });
        await _sut.AddBankAccountAsync(1, new AddBankAccountDto(2, "TR000000000000000000000099"));

        captured!.IsDefault.Should().BeFalse();
    }

    // ────────────────── DeleteBankAccountAsync ──────────────────

    [Fact]
    public async Task DeleteBankAccountAsync_CallsRepositoryDelete_WhenAccountExists()
    {
        var account = MakeBankAccount();
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 5)).ReturnsAsync(account);

        await _sut.DeleteBankAccountAsync(subjectId: 1, accountId: 5);

        _walletRepoMock.Verify(r => r.DeleteBankAccountAsync(account), Times.Once);
    }

    [Fact]
    public async Task DeleteBankAccountAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        _walletRepoMock.Setup(r => r.GetBankAccountAsync(1, 99)).ReturnsAsync((BankAccount?)null);

        await _sut.Invoking(s => s.DeleteBankAccountAsync(1, 99))
            .Should().ThrowAsync<NotFoundException>();
    }
}
