using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISpecialDayCalendarService
{
    Task<List<SpecialDayDto>> GetByYearAsync(int year);
    Task<SpecialDaySyncResultDto> SyncYearAsync(int year, int adminId);
    Task<List<string>> GetEventCodesByDateAsync(DateOnly date);
    Task SaveManualDayAsync(SaveSpecialDayDto dto, int adminId);
    Task DeleteManualDayAsync(int id, int adminId);
}
