using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record ProjectAssignmentDto(
    int SubjectId,
    string FullName,
    string PhoneNumber,
    string CityName,
    Gender Gender,
    int Age,
    string? SocioeconomicStatusCode,
    AssignmentStatus Status,
    DateTime AssignedAt,
    DateTime? CompletedAt,
    decimal EarnedAmount,
    RewardStatus RewardStatus);

public record SubjectAssignmentJobDto(
    int Id,
    int RequestedCount,
    int AssignedCount,
    int SkippedCount,
    AssignmentJobStatus Status,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record RewardProcessResultDto(
    int RequestedCount,
    int ProcessedCount,
    int SkippedCount,
    decimal ProcessedAmount);

public record RemoveAssignmentsResultDto(
    int RequestedCount,
    int RemovedCount,
    int SkippedCount);
