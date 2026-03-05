using FluentValidation;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class AuthService(
    ISubjectRepository subjectRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    ICacheService cache,
    ISmsService smsService,
    IMessagePublisher publisher,
    IValidator<LoginDto> loginValidator,
    IValidator<RegisterSubjectDto> registerValidator,
    ILogger<AuthService> logger) : IAuthService
{
    private static string OtpKey(string phone)           => $"otp:{phone}";
    private static string RegTokenKey(string token)       => $"reg:{token}";
    private static string SessionKey(int subjectId)       => $"session:{subjectId}";
    private static string BlacklistKey(string phone)      => $"otp:blacklist:{phone}";
    private static string OtpAttemptsKey(string phone)    => $"otp:attempts:{phone}";
    private static string VerifyFailKey(string phone)     => $"otp:verify:fail:{phone}";

    private const int MaxOtpRequestsPerHour = 5;
    private const int MaxVerifyFailsPerWindow = 3;

    public async Task SendOtpAsync(string phoneNumber)
    {
        if (await cache.ExistsAsync(BlacklistKey(phoneNumber)))
            throw new BusinessException("BLACKLISTED", "Bu numara kara listeye alınmıştır. Lütfen destek ile iletişime geçin.");

        if (await subjectRepository.ExistsByPhoneAsync(phoneNumber))
            throw new BusinessException("ALREADY_EXISTS", "Bu telefon numarası zaten kayıtlı.");

        var attempts = await cache.IncrementAsync(OtpAttemptsKey(phoneNumber), TimeSpan.FromHours(1));

        if (attempts > MaxOtpRequestsPerHour)
        {
            await cache.SetAsync(true, BlacklistKey(phoneNumber));
            logger.LogWarning("OTP limit aşıldı, kara listeye alındı: {PhoneNumber}", phoneNumber);
            throw new BusinessException("BLACKLISTED", "Çok fazla OTP isteği gönderildi. Numara kara listeye alındı.");
        }

        var otp = await smsService.SendOtpAsync(phoneNumber);
        await cache.SetAsync(otp, OtpKey(phoneNumber), TimeSpan.FromMinutes(5));

        logger.LogInformation("OTP gönderildi: {PhoneNumber} (deneme: {Attempt}/{Max})", phoneNumber, attempts, MaxOtpRequestsPerHour);
    }

    public async Task<OtpVerifiedDto> VerifyOtpAsync(string phoneNumber, string otp)
    {
        if (await cache.ExistsAsync(BlacklistKey(phoneNumber)))
            throw new BusinessException("BLACKLISTED", "Bu numara kara listeye alınmıştır. Lütfen destek ile iletişime geçin.");

        var stored = await cache.GetAsync<string>(OtpKey(phoneNumber));

        if (stored is null || stored != otp)
        {
            var fails = await cache.IncrementAsync(VerifyFailKey(phoneNumber), TimeSpan.FromMinutes(15));

            if (fails >= MaxVerifyFailsPerWindow)
            {
                await cache.SetAsync(true, BlacklistKey(phoneNumber));
                await cache.RemoveAsync(OtpKey(phoneNumber));
                logger.LogWarning("OTP doğrulama limiti aşıldı, kara listeye alındı: {PhoneNumber}", phoneNumber);
                throw new BusinessException("BLACKLISTED", "Çok fazla hatalı OTP denemesi. Numara kara listeye alındı.");
            }

            throw new BusinessException("INVALID_OTP", "Geçersiz veya süresi dolmuş OTP.");
        }

        await cache.RemoveAsync(OtpKey(phoneNumber));
        await cache.RemoveAsync(VerifyFailKey(phoneNumber));

        var token = Guid.NewGuid().ToString("N");
        await cache.SetAsync(phoneNumber, RegTokenKey(token), TimeSpan.FromMinutes(15));

        logger.LogInformation("OTP doğrulandı: {PhoneNumber}", phoneNumber);

        return new OtpVerifiedDto(token);
    }

    public async Task QueueRegistrationAsync(RegisterSubjectDto dto)
    {
        await registerValidator.ValidateAndThrowAsync(dto);

        var phoneNumber = await cache.GetAsync<string>(RegTokenKey(dto.RegistrationToken));

        if (phoneNumber is null)
            throw new BusinessException("INVALID_TOKEN", "Geçersiz veya süresi dolmuş kayıt tokeni.");

        if (await subjectRepository.ExistsByPhoneAsync(phoneNumber))
            throw new BusinessException("ALREADY_EXISTS", "Bu telefon numarası zaten kayıtlı.");

        if (await subjectRepository.ExistsByEmailAsync(dto.Email))
            throw new BusinessException("ALREADY_EXISTS", "Bu e-posta adresi zaten kayıtlı.");

        await cache.RemoveAsync(RegTokenKey(dto.RegistrationToken));

        var passwordHash = passwordHasher.Hash(dto.Password);

        var message = new SubjectRegisteredMessage(
            Email: dto.Email,
            FirstName: dto.FirstName,
            LastName: dto.LastName,
            PasswordHash: passwordHash,
            PhoneNumber: phoneNumber,
            Gender: (int)dto.Gender,
            MaritalStatus: (int)dto.MaritalStatus,
            GsmOperator: (int)dto.GsmOperator,
            BirthDate: dto.BirthDate,
            CityId: dto.CityId,
            DistrictId: dto.DistrictId,
            IsRetired: dto.IsRetired,
            ProfessionId: dto.ProfessionId,
            EducationLevelId: dto.EducationLevelId,
            IsHeadOfFamily: dto.IsHeadOfFamily,
            IsHeadOfFamilyRetired: dto.IsHeadOfFamilyRetired,
            HeadOfFamilyProfessionId: dto.HeadOfFamilyProfessionId,
            HeadOfFamilyEducationLevelId: dto.HeadOfFamilyEducationLevelId,
            BankId: dto.BankId,
            IBAN: dto.IBAN,
            IBANFullName: dto.IBANFullName,
            SocioeconomicStatusId: 1, // Worker hesaplar
            LSMSocioeconomicStatusId: 1, // TODO: LSM formülü gelince
            ReferenceCode: dto.ReferenceCode,
            SpecialCodeId: null, // Admin sonradan atar
            KVKKApproval: dto.KVKKApproval,
            KVKKDetail: dto.KVKKDetail,
            RegisteredAt: DateTime.UtcNow);

        await publisher.PublishAsync(message, Queues.SubjectRegistered);

        logger.LogInformation("Denek kaydı kuyruğa alındı: {Email}", dto.Email);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        await loginValidator.ValidateAndThrowAsync(dto);

        var subject = await subjectRepository.GetByEmailAsync(dto.Email);

        if (subject is null || !passwordHasher.Verify(dto.Password, subject.PasswordHash))
        {
            logger.LogWarning("Başarısız giriş denemesi: {Email}", dto.Email);
            throw new BusinessException("INVALID_CREDENTIALS", "E-posta veya şifre hatalı.");
        }

        if (subject.Status != ApprovalStatus.Approved)
        {
            logger.LogWarning("Onaylı olmayan denek girişi: {SubjectId}", subject.Id);
            throw new BusinessException("ACCOUNT_NOT_APPROVED", "Hesabınız henüz onaylanmamış.");
        }

        var accessToken = jwtService.GenerateAccessToken(subject);
        var refreshToken = CreateRefreshToken(subject.Id);

        await refreshTokenRepository.AddAsync(refreshToken);
        await cache.SetAsync(accessToken, SessionKey(subject.Id), TimeSpan.FromDays(7));

        logger.LogInformation("Denek girişi başarılı: {SubjectId}", subject.Id);

        return new AuthResultDto(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        var stored = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored is null || !stored.IsActive)
        {
            if (stored?.IsRevoked == true)
            {
                logger.LogWarning("Refresh token yeniden kullanım tespiti: SubjectId={SubjectId}", stored.SubjectId);
                await refreshTokenRepository.RevokeAllForSubjectAsync(stored.SubjectId, "Token reuse detected");
            }
            throw new BusinessException("INVALID_TOKEN", "Geçersiz veya süresi dolmuş refresh token.");
        }

        var subject = await subjectRepository.GetByIdAsync(stored.SubjectId)
            ?? throw new NotFoundException("Denek");

        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedReason = "Replaced by new token";

        var newRefreshToken = CreateRefreshToken(subject.Id);
        stored.ReplacedByToken = newRefreshToken.Token;

        await refreshTokenRepository.UpdateAsync(stored);
        await refreshTokenRepository.AddAsync(newRefreshToken);

        var accessToken = jwtService.GenerateAccessToken(subject);
        await cache.SetAsync(accessToken, SessionKey(subject.Id), TimeSpan.FromDays(7));

        logger.LogInformation("Token yenilendi: {SubjectId}", subject.Id);

        return new AuthResultDto(accessToken, newRefreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task LogoutAsync(int subjectId)
    {
        await cache.RemoveAsync(SessionKey(subjectId));
        await refreshTokenRepository.RevokeAllForSubjectAsync(subjectId, "User logout");
        logger.LogInformation("Denek çıkış yaptı: {SubjectId}", subjectId);
    }

    public async Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new BusinessException("INVALID_PASSWORD", "Şifre en az 6 karakter olmalıdır.");

        var verifyResult = await VerifyOtpAsync(phoneNumber, otp);

        var subject = await subjectRepository.GetByPhoneAsync(phoneNumber)
            ?? throw new NotFoundException("Bu telefon numarasına ait hesap bulunamadı.");

        subject.PasswordHash = passwordHasher.Hash(newPassword);
        await subjectRepository.UpdateAsync(subject);

        await cache.RemoveAsync(RegTokenKey(verifyResult.RegistrationToken));

        logger.LogInformation("Şifre sıfırlandı: {SubjectId}", subject.Id);
    }

    public async Task SendPasswordResetOtpAsync(string phoneNumber)
    {
        if (!await subjectRepository.ExistsByPhoneAsync(phoneNumber))
            throw new BusinessException("NOT_FOUND", "Bu telefon numarasına ait hesap bulunamadı.");

        var attempts = await cache.IncrementAsync(OtpAttemptsKey(phoneNumber), TimeSpan.FromHours(1));

        if (attempts > MaxOtpRequestsPerHour)
            throw new BusinessException("TOO_MANY_REQUESTS", "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyin.");

        var otpCode = await smsService.SendOtpAsync(phoneNumber);
        await cache.SetAsync(otpCode, OtpKey(phoneNumber), TimeSpan.FromMinutes(5));

        logger.LogInformation("Şifre sıfırlama OTP gönderildi: {PhoneNumber}", phoneNumber);
    }

    private static RefreshToken CreateRefreshToken(int subjectId)
    {
        var bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(bytes),
            SubjectId = subjectId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }
}
