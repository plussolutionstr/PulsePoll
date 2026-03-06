using Microsoft.Extensions.Logging;
using PulsePoll.Application.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class SubjectService(
    ISubjectRepository repository,
    ISubjectScoreService scoreService,
    IReferralRewardService referralRewardService,
    IStorageService storageService,
    IMediaUrlService mediaUrlService,
    IMessagePublisher publisher,
    ICacheService cache,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<SubjectService> logger) : ISubjectService
{
    public async Task<SubjectDto?> GetByIdAsync(int id)
    {
        var subject = await repository.GetByIdAsync(id);
        if (subject is null)
            return null;

        var score = await scoreService.GetCurrentAsync(subject.Id);
        return await MapToDtoAsync(subject, score);
    }

    public async Task<SubjectDto?> GetByEmailAsync(string email)
    {
        var subject = await repository.GetByEmailAsync(email);
        if (subject is null)
            return null;

        var score = await scoreService.GetCurrentAsync(subject.Id);
        return await MapToDtoAsync(subject, score);
    }

    public Task<bool> ExistsAsync(string email)
        => repository.ExistsByEmailAsync(email);

    public async Task UpdateFcmTokenAsync(int subjectId, string fcmToken)
    {
        var subject = await repository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        subject.FcmToken = fcmToken;
        await repository.UpdateAsync(subject);

        logger.LogInformation("FCM token güncellendi: {SubjectId}", subjectId);
    }

    public async Task UpdateProfileAsync(int subjectId, UpdateProfileDto dto)
    {
        var subject = await repository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        subject.FirstName = dto.FirstName.ToTitleCaseTr();
        subject.LastName = dto.LastName.ToTitleCaseTr();
        subject.Email = dto.Email?.Trim();
        subject.Gender = dto.Gender;
        subject.BirthDate = dto.BirthDate;
        subject.MaritalStatus = dto.MaritalStatus;
        subject.GsmOperator = dto.GsmOperator;
        subject.CityId = dto.CityId;
        subject.DistrictId = dto.DistrictId;
        subject.ProfessionId = dto.ProfessionId;
        subject.EducationLevelId = dto.EducationLevelId;
        subject.IsRetired = dto.IsRetired;
        subject.IsHeadOfFamily = dto.IsHeadOfFamily;
        subject.IsHeadOfFamilyRetired = dto.IsHeadOfFamilyRetired;
        subject.HeadOfFamilyProfessionId = dto.IsHeadOfFamily ? null : dto.HeadOfFamilyProfessionId;
        subject.HeadOfFamilyEducationLevelId = dto.IsHeadOfFamily ? null : dto.HeadOfFamilyEducationLevelId;

        await repository.UpdateAsync(subject);
        logger.LogInformation("Profil güncellendi: {SubjectId}", subjectId);
    }

    public async Task<string> UploadProfilePhotoAsync(int subjectId, Stream stream, string contentType, string fileName)
    {
        var subject = await repository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        var ext = Path.GetExtension(fileName);
        var objectName = $"{subjectId}{ext}";

        if (!string.IsNullOrEmpty(subject.ProfilePhotoUrl))
        {
            try
            {
                var oldKey = subject.ProfilePhotoUrl.StartsWith("profile-photos/")
                    ? subject.ProfilePhotoUrl["profile-photos/".Length..]
                    : subject.ProfilePhotoUrl;
                await storageService.DeleteAsync("profile-photos", oldKey);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Önceki profil fotoğrafı silinemedi, yeni yükleme ile devam ediliyor: SubjectId={SubjectId}",
                    subjectId);
            }
        }

        await storageService.UploadAsync("profile-photos", objectName, stream, contentType);

        subject.ProfilePhotoUrl = objectName;
        await repository.UpdateAsync(subject);

        var photoUrl = await mediaUrlService.GetMediaUrlAsync("profile-photos", objectName);
        logger.LogInformation("Profil fotoğrafı yüklendi: {SubjectId}", subjectId);
        return photoUrl;
    }

    public async Task<List<SubjectDto>> GetAllAsync()
    {
        var subjects = await repository.GetAllAsync();
        var scoreMap = await scoreService.GetCurrentBulkAsync(subjects.Select(s => s.Id));
        return subjects
            .Select(s => MapToDto(s, scoreMap.GetValueOrDefault(s.Id)))
            .ToList();
    }

    public async Task ApproveAsync(int id, int adminId)
    {
        var subject = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Denek");

        subject.Status = ApprovalStatus.Approved;
        await repository.UpdateAsync(subject);

        await referralRewardService.TryGrantAsync(
            id,
            ReferralRewardTriggerType.AccountApproved,
            adminId);

        logger.LogInformation("Denek onaylandı: {SubjectId} by Admin {AdminId}", id, adminId);
    }

    public async Task RejectAsync(int id, int adminId)
    {
        var subject = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Denek");

        subject.Status = ApprovalStatus.Rejected;
        await repository.UpdateAsync(subject);

        await cache.RemoveAsync($"session:{id}");
        await refreshTokenRepository.RevokeAllForSubjectAsync(id, "Account rejected by admin");

        logger.LogInformation("Denek reddedildi: {SubjectId} by Admin {AdminId}", id, adminId);
    }

    public async Task SendBulkSmsAsync(IEnumerable<int> subjectIds, string message, int sentByAdminId)
    {
        var ids = subjectIds.ToList();
        var targets = await repository.GetByIdsAsync(ids);

        foreach (var s in targets)
            await publisher.PublishAsync(
                new SmsSendMessage(s.PhoneNumber, message, s.Id, sentByAdminId),
                Queues.SmsSend);

        logger.LogInformation("Toplu SMS gönderildi: {Count} denek by Admin {AdminId}", targets.Count, sentByAdminId);
    }

    private async Task<SubjectDto> MapToDtoAsync(Domain.Entities.Subject s, SubjectScoreDto? score = null)
    {
        string? photoUrl = null;
        if (!string.IsNullOrEmpty(s.ProfilePhotoUrl))
        {
            try
            {
                var key = s.ProfilePhotoUrl.StartsWith("profile-photos/")
                    ? s.ProfilePhotoUrl["profile-photos/".Length..]
                    : s.ProfilePhotoUrl;
                photoUrl = await mediaUrlService.GetMediaUrlAsync("profile-photos", key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Profil fotoğrafı URL'i üretilemedi: SubjectId={SubjectId}",
                    s.Id);
            }
        }

        return BuildDto(s, score, photoUrl);
    }

    private static SubjectDto MapToDto(Domain.Entities.Subject s, SubjectScoreDto? score = null)
        => BuildDto(s, score, s.ProfilePhotoUrl);

    private static SubjectDto BuildDto(Domain.Entities.Subject s, SubjectScoreDto? score, string? photoUrl)
    {
        var completed = score?.Completed ?? 0;
        var disqualified = (score?.Disqualify ?? 0) + (score?.ScreenOut ?? 0);
        var total = score?.Started ?? 0;
        var successRate = total > 0 ? (int)Math.Round(100.0 * completed / total) : 0;

        return new SubjectDto(
            s.Id,
            s.PublicId,
            s.Email,
            s.FirstName,
            s.LastName,
            s.FullName,
            s.PhoneNumber,
            s.Gender,
            s.Age,
            s.BirthDate,
            s.MaritalStatus,
            s.GsmOperator,
            s.City?.Name ?? string.Empty,
            s.District?.Name ?? string.Empty,
            s.Profession?.Name ?? string.Empty,
            s.EducationLevel?.Name ?? string.Empty,
            s.IsRetired,
            s.IsHeadOfFamily,
            s.IsHeadOfFamilyRetired,
            s.HeadOfFamilyProfession?.Name,
            s.HeadOfFamilyEducationLevel?.Name,
            ResolvePrimaryBankName(s),
            ResolvePrimaryIban(s),
            string.Empty,
            s.SocioeconomicStatus?.Name ?? string.Empty,
            s.SocioeconomicStatus?.Code,
            s.LSMSocioeconomicStatus?.Name ?? string.Empty,
            s.LSMSocioeconomicStatus?.Code,
            s.ReferenceCode,
            s.ReferralCode,
            photoUrl,
            s.Status,
            s.CreatedAt,
            score?.Score,
            score?.Star,
            completed,
            disqualified,
            successRate);
    }

    private static string ResolvePrimaryBankName(Domain.Entities.Subject subject)
        => subject.BankAccounts
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.CreatedAt)
            .Select(b => b.BankName)
            .FirstOrDefault() ?? string.Empty;

    private static string ResolvePrimaryIban(Domain.Entities.Subject subject)
        => subject.BankAccounts
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.CreatedAt)
            .Select(b => b.IbanEncrypted)
            .FirstOrDefault() ?? string.Empty;
}
