using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectService
{
    Task<SubjectDto?> GetByIdAsync(int id);
    Task<SubjectDto?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
    Task UpdateFcmTokenAsync(int subjectId, string fcmToken);
    Task UpdateProfileAsync(int subjectId, UpdateProfileDto dto);
    Task<string> UploadProfilePhotoAsync(int subjectId, Stream stream, string contentType, string fileName);
    Task<List<SubjectDto>> GetAllAsync();
    Task ApproveAsync(int id, int adminId);
    Task RejectAsync(int id, int adminId);
    Task SendBulkSmsAsync(IEnumerable<int> subjectIds, string message, int sentByAdminId);
}
