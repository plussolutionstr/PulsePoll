using MassTransit;
using PulsePoll.Application.Helpers;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using PulsePoll.Infrastructure.Notifications;

namespace PulsePoll.Worker.Consumers;

public class SubjectRegisteredConsumer(
    ILogger<SubjectRegisteredConsumer> logger,
    ISubjectRepository subjectRepository,
    IWalletRepository walletRepository,
    ILookupService lookupService,
    IReferralRewardService referralRewardService,
    ISesCalculator sesCalculator,
    SmtpMailService mailService) : IConsumer<SubjectRegisteredMessage>
{
    public async Task Consume(ConsumeContext<SubjectRegisteredMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Denek kaydı işleniyor: {Email}", msg.Email);

        var sesId = await sesCalculator.CalculateSesIdAsync(
            professionId: msg.ProfessionId,
            educationLevelId: msg.EducationLevelId,
            isRetired: msg.IsRetired,
            isHeadOfFamily: msg.IsHeadOfFamily,
            headOfFamilyProfessionId: msg.HeadOfFamilyProfessionId,
            headOfFamilyEducationLevelId: msg.HeadOfFamilyEducationLevelId,
            isHeadOfFamilyRetired: msg.IsHeadOfFamilyRetired);

        if (sesId is null)
            logger.LogWarning("SES hesaplanamadı: {Email}", msg.Email);

        // TODO: LSM hesaplaması — formül gelince eklenecek
        const int lsmId = 1;

        var subject = new Subject
        {
            Email = msg.Email,
            FirstName = msg.FirstName,
            LastName = msg.LastName,
            PasswordHash = msg.PasswordHash,
            PhoneNumber = msg.PhoneNumber,
            Gender = (Gender)msg.Gender,
            MaritalStatus = (MaritalStatus)msg.MaritalStatus,
            GsmOperator = (GsmOperator)msg.GsmOperator,
            BirthDate = msg.BirthDate,
            CityId = msg.CityId,
            DistrictId = msg.DistrictId,
            IsRetired = msg.IsRetired,
            ProfessionId = msg.ProfessionId,
            EducationLevelId = msg.EducationLevelId,
            IsHeadOfFamily = msg.IsHeadOfFamily,
            IsHeadOfFamilyRetired = msg.IsHeadOfFamilyRetired,
            HeadOfFamilyProfessionId = msg.HeadOfFamilyProfessionId,
            HeadOfFamilyEducationLevelId = msg.HeadOfFamilyEducationLevelId,
            SocioeconomicStatusId = sesId ?? 1,
            LSMSocioeconomicStatusId = lsmId,
            ReferenceCode = msg.ReferenceCode,
            ReferralCode = ReferralCodeGenerator.Generate(),
            SpecialCodeId = msg.SpecialCodeId,
            KVKKApproval = msg.KVKKApproval,
            KVKKDetail = msg.KVKKDetail,
            KVKKApprovalDate = msg.KVKKApproval ? msg.RegisteredAt : null,
            Status = ApprovalStatus.Approved
        };

        subject.SetCreated(0, msg.RegisteredAt);

        await subjectRepository.AddAsync(subject);
        logger.LogInformation("Denek DB'ye yazıldı: {SubjectId}", subject.Id);

        var wallet = new Wallet { SubjectId = subject.Id };
        wallet.SetCreated(subject.Id, msg.RegisteredAt);
        await walletRepository.AddAsync(wallet);
        logger.LogInformation("Cüzdan oluşturuldu: {SubjectId}", subject.Id);

        if (msg.BankId.HasValue && !string.IsNullOrWhiteSpace(msg.IBAN))
        {
            var bank = await lookupService.GetBankByIdAsync(msg.BankId.Value);
            if (bank is not null)
            {
                var account = new BankAccount
                {
                    SubjectId = subject.Id,
                    BankName = bank.Name,
                    IbanEncrypted = msg.IBAN,
                    IbanLast4 = msg.IBAN.Length >= 4 ? msg.IBAN[^4..] : msg.IBAN,
                    IsDefault = true
                };
                account.SetCreated(subject.Id, msg.RegisteredAt);
                await walletRepository.AddBankAccountAsync(account);
                logger.LogInformation("Kayıt sırasında banka hesabı eklendi: SubjectId={SubjectId}", subject.Id);
            }
        }

        await TryCreateReferralAsync(subject, msg.ReferenceCode, msg.RegisteredAt);
        await referralRewardService.TryGrantAsync(
            subject.Id,
            ReferralRewardTriggerType.RegistrationCompleted,
            actorId: 0);

        await mailService.SendAsync(
            msg.Email,
            "PulsePoll'a Hoşgeldiniz!",
            $"<h1>Merhaba {msg.FirstName},</h1><p>Hesabınız oluşturuldu ve aktif edildi.</p>");

        logger.LogInformation("Denek kaydı tamamlandı: {SubjectId}", subject.Id);
    }

    private async Task TryCreateReferralAsync(Subject referredSubject, string? referenceCode, DateTime registeredAt)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            return;

        var normalizedCode = referenceCode.Trim().ToUpperInvariant();
        var referrer = await subjectRepository.GetByReferralCodeAsync(normalizedCode);
        if (referrer is null)
        {
            logger.LogWarning("Referans kodu bulunamadı: SubjectId={SubjectId} ReferenceCode={ReferenceCode}",
                referredSubject.Id, normalizedCode);
            return;
        }

        if (referrer.Id == referredSubject.Id)
        {
            logger.LogWarning("Denek kendi referans kodunu girdi: SubjectId={SubjectId}", referredSubject.Id);
            return;
        }

        var existing = await subjectRepository.GetReferralByReferredSubjectIdAsync(referredSubject.Id);
        if (existing is not null)
            return;

        var referral = new Referral
        {
            ReferrerId = referrer.Id,
            ReferredSubjectId = referredSubject.Id,
            ReferredAt = registeredAt
        };
        referral.SetCreated(referredSubject.Id, registeredAt);
        await subjectRepository.AddReferralAsync(referral);

        logger.LogInformation("Referral oluşturuldu: ReferrerId={ReferrerId} ReferredSubjectId={ReferredSubjectId}",
            referrer.Id, referredSubject.Id);
    }
}
