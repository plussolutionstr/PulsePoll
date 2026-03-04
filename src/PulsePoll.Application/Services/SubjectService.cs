using Microsoft.Extensions.Logging;
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
    IMessagePublisher publisher,
    ILogger<SubjectService> logger) : ISubjectService
{
    public async Task<SubjectDto?> GetByIdAsync(int id)
    {
        var subject = await repository.GetByIdAsync(id);
        if (subject is null)
            return null;

        var score = await scoreService.GetCurrentAsync(subject.Id);
        return MapToDto(subject, score);
    }

    public async Task<SubjectDto?> GetByEmailAsync(string email)
    {
        var subject = await repository.GetByEmailAsync(email);
        if (subject is null)
            return null;

        var score = await scoreService.GetCurrentAsync(subject.Id);
        return MapToDto(subject, score);
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

        logger.LogInformation("Denek reddedildi: {SubjectId} by Admin {AdminId}", id, adminId);
    }

    public async Task SendBulkSmsAsync(IEnumerable<int> subjectIds, string message, int sentByAdminId)
    {
        var ids = subjectIds.ToList();
        var subjects = await repository.GetAllAsync();
        var targets = subjects.Where(s => ids.Contains(s.Id)).ToList();

        foreach (var s in targets)
            await publisher.PublishAsync(
                new SmsSendMessage(s.PhoneNumber, message, s.Id, sentByAdminId),
                Queues.SmsSend);

        logger.LogInformation("Toplu SMS gönderildi: {Count} denek by Admin {AdminId}", targets.Count, sentByAdminId);
    }

    private static SubjectDto MapToDto(Domain.Entities.Subject s, SubjectScoreDto? score = null) => new(
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
        s.Bank?.Name ?? string.Empty,
        s.IBAN,
        s.IBANFullName,
        s.SocioeconomicStatus?.Name ?? string.Empty,
        s.SocioeconomicStatus?.Code,
        s.LSMSocioeconomicStatus?.Name ?? string.Empty,
        s.LSMSocioeconomicStatus?.Code,
        s.ReferenceCode,
        s.ReferralCode,
        s.Status,
        s.CreatedAt,
        score?.Score,
        score?.Star);
}
