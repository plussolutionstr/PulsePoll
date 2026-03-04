using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record SpecialDayDto(
    int Id,
    string EventCode,
    string Name,
    DateOnly Date,
    SpecialDayCategory Category,
    string Source,
    bool IsActive);

public record SpecialDaySyncResultDto(
    int Year,
    int Count,
    int NationalCount,
    int ReligiousCount,
    int SpecialCount);

public record SaveSpecialDayDto(
    int? Id,
    string EventCode,
    string Name,
    DateOnly Date,
    SpecialDayCategory Category,
    bool IsActive);
