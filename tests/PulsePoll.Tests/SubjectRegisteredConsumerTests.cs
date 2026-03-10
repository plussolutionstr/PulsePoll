using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using PulsePoll.Infrastructure.Notifications;
using PulsePoll.Worker.Consumers;
using Xunit;

namespace PulsePoll.Tests;

public class SubjectRegisteredConsumerTests
{
    private readonly Mock<ILogger<SubjectRegisteredConsumer>> _loggerMock = new();
    private readonly Mock<ISubjectRepository> _subjectRepositoryMock = new();
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<ILookupService> _lookupServiceMock = new();
    private readonly Mock<IRegistrationConfigService> _registrationConfigServiceMock = new();
    private readonly Mock<IReferralRewardService> _referralRewardServiceMock = new();
    private readonly Mock<IAffiliateRewardService> _affiliateRewardServiceMock = new();
    private readonly Mock<IExternalAffiliateRepository> _externalAffiliateRepositoryMock = new();
    private readonly Mock<ISubjectScoreService> _subjectScoreServiceMock = new();
    private readonly Mock<ISesCalculator> _sesCalculatorMock = new();
    private readonly Mock<ILogger<SmtpMailService>> _mailLoggerMock = new();

    [Fact]
    public async Task Consume_WhenSubjectIsAutoApproved_TriggersApprovalReferralReward()
    {
        var sut = new SubjectRegisteredConsumer(
            _loggerMock.Object,
            _subjectRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _lookupServiceMock.Object,
            _registrationConfigServiceMock.Object,
            _referralRewardServiceMock.Object,
            _affiliateRewardServiceMock.Object,
            _externalAffiliateRepositoryMock.Object,
            _subjectScoreServiceMock.Object,
            _sesCalculatorMock.Object,
            new SmtpMailService(_mailLoggerMock.Object));

        var registeredAt = new DateTime(2026, 3, 8, 12, 0, 0, DateTimeKind.Utc);
        var message = new SubjectRegisteredMessage(
            Email: "test@example.com",
            FirstName: "Ali",
            LastName: "Yilmaz",
            PasswordHash: "hash",
            PhoneNumber: "905551112233",
            Gender: (int)Gender.Male,
            MaritalStatus: (int)MaritalStatus.Single,
            GsmOperator: (int)GsmOperator.Turkcell,
            BirthDate: new DateOnly(1990, 1, 1),
            CityId: 1,
            DistrictId: 1,
            IsRetired: false,
            ProfessionId: 1,
            EducationLevelId: 1,
            IsHeadOfFamily: true,
            IsHeadOfFamilyRetired: false,
            HeadOfFamilyProfessionId: null,
            HeadOfFamilyEducationLevelId: null,
            BankId: null,
            IBAN: null,
            IBANFullName: null,
            SocioeconomicStatusId: 1,
            LSMSocioeconomicStatusId: 1,
            ReferenceCode: null,
            SpecialCodeId: null,
            KVKKApproval: true,
            KVKKDetail: "Onay",
            RegisteredAt: registeredAt);

        _sesCalculatorMock
            .Setup(x => x.CalculateSesIdAsync(
                message.ProfessionId,
                message.EducationLevelId,
                message.IsRetired,
                message.IsHeadOfFamily,
                message.HeadOfFamilyProfessionId,
                message.HeadOfFamilyEducationLevelId,
                message.IsHeadOfFamilyRetired))
            .ReturnsAsync(1);
        _registrationConfigServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new Application.DTOs.RegistrationConfigDto(true));

        _subjectRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Subject>()))
            .Callback<Subject>(subject => subject.Id = 42)
            .Returns(Task.CompletedTask);

        _walletRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wallet>()))
            .Returns(Task.CompletedTask);

        _subjectScoreServiceMock
            .Setup(x => x.RecalculateAsync(42))
            .Returns(Task.CompletedTask);

        var contextMock = new Mock<ConsumeContext<SubjectRegisteredMessage>>();
        contextMock.SetupGet(x => x.Message).Returns(message);

        await sut.Consume(contextMock.Object);

        _referralRewardServiceMock.Verify(
            x => x.TryGrantAsync(42, ReferralRewardTriggerType.RegistrationCompleted, 0),
            Times.Once);

        _referralRewardServiceMock.Verify(
            x => x.TryGrantAsync(42, ReferralRewardTriggerType.AccountApproved, 0),
            Times.Once);

        _subjectRepositoryMock.Invocations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Consume_WhenAutoApproveDisabled_DoesNotTriggerApprovalReward()
    {
        var sut = new SubjectRegisteredConsumer(
            _loggerMock.Object,
            _subjectRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _lookupServiceMock.Object,
            _registrationConfigServiceMock.Object,
            _referralRewardServiceMock.Object,
            _affiliateRewardServiceMock.Object,
            _externalAffiliateRepositoryMock.Object,
            _subjectScoreServiceMock.Object,
            _sesCalculatorMock.Object,
            new SmtpMailService(_mailLoggerMock.Object));

        var message = new SubjectRegisteredMessage(
            Email: "test2@example.com",
            FirstName: "Ayse",
            LastName: "Yilmaz",
            PasswordHash: "hash",
            PhoneNumber: "905551112234",
            Gender: (int)Gender.Female,
            MaritalStatus: (int)MaritalStatus.Single,
            GsmOperator: (int)GsmOperator.Turkcell,
            BirthDate: new DateOnly(1991, 2, 2),
            CityId: 1,
            DistrictId: 1,
            IsRetired: false,
            ProfessionId: 1,
            EducationLevelId: 1,
            IsHeadOfFamily: true,
            IsHeadOfFamilyRetired: false,
            HeadOfFamilyProfessionId: null,
            HeadOfFamilyEducationLevelId: null,
            BankId: null,
            IBAN: null,
            IBANFullName: null,
            SocioeconomicStatusId: 1,
            LSMSocioeconomicStatusId: 1,
            ReferenceCode: null,
            SpecialCodeId: null,
            KVKKApproval: true,
            KVKKDetail: "Onay",
            RegisteredAt: new DateTime(2026, 3, 8, 12, 30, 0, DateTimeKind.Utc));

        _sesCalculatorMock
            .Setup(x => x.CalculateSesIdAsync(
                message.ProfessionId,
                message.EducationLevelId,
                message.IsRetired,
                message.IsHeadOfFamily,
                message.HeadOfFamilyProfessionId,
                message.HeadOfFamilyEducationLevelId,
                message.IsHeadOfFamilyRetired))
            .ReturnsAsync(1);
        _registrationConfigServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new Application.DTOs.RegistrationConfigDto(false));

        Subject? savedSubject = null;
        _subjectRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Subject>()))
            .Callback<Subject>(subject =>
            {
                subject.Id = 43;
                savedSubject = subject;
            })
            .Returns(Task.CompletedTask);

        _walletRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wallet>()))
            .Returns(Task.CompletedTask);

        _subjectScoreServiceMock
            .Setup(x => x.RecalculateAsync(43))
            .Returns(Task.CompletedTask);

        var contextMock = new Mock<ConsumeContext<SubjectRegisteredMessage>>();
        contextMock.SetupGet(x => x.Message).Returns(message);

        await sut.Consume(contextMock.Object);

        savedSubject.Should().NotBeNull();
        savedSubject!.Status.Should().Be(ApprovalStatus.Pending);
        _referralRewardServiceMock.Verify(
            x => x.TryGrantAsync(43, ReferralRewardTriggerType.RegistrationCompleted, 0),
            Times.Once);
        _referralRewardServiceMock.Verify(
            x => x.TryGrantAsync(43, ReferralRewardTriggerType.AccountApproved, 0),
            Times.Never);
    }
}
