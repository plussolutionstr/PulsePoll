using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class SpecialDayCalendarService(
    ISpecialDayRepository repository,
    ILogger<SpecialDayCalendarService> logger) : ISpecialDayCalendarService
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    public async Task<List<SpecialDayDto>> GetByYearAsync(int year)
    {
        var rows = await repository.GetByYearAsync(year);
        return rows
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<SpecialDaySyncResultDto> SyncYearAsync(int year, int adminId)
    {
        var all = new List<SpecialDay>();
        all.AddRange(BuildNationalDays(year));
        all.AddRange(BuildSpecialDays(year));
        all.AddRange(await BuildReligiousDaysAsync(year));

        var merged = all
            .GroupBy(x => new { x.EventCode, x.Date })
            .Select(g => g.First())
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Name)
            .ToList();

        foreach (var day in merged)
            day.SetCreated(adminId);

        await repository.ReplaceSystemYearAsync(year, merged);

        var national = merged.Count(x => x.Category == SpecialDayCategory.National);
        var religious = merged.Count(x => x.Category == SpecialDayCategory.Religious);
        var special = merged.Count(x => x.Category == SpecialDayCategory.Special);

        logger.LogInformation(
            "Özel gün takvimi senkronize edildi: Year={Year} Total={Total} National={National} Religious={Religious} Special={Special}",
            year, merged.Count, national, religious, special);

        return new SpecialDaySyncResultDto(year, merged.Count, national, religious, special);
    }

    public async Task<List<string>> GetEventCodesByDateAsync(DateOnly date)
    {
        var rows = await repository.GetByDateAsync(date);
        return rows
            .Where(x => x.IsActive)
            .Select(x => x.EventCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task SaveManualDayAsync(SaveSpecialDayDto dto, int adminId)
    {
        ValidateManual(dto);

        var normalizedCode = dto.EventCode.Trim().ToUpperInvariant();
        var normalizedName = dto.Name.Trim();

        if (dto.Id is null or <= 0)
        {
            if (await repository.ExistsByEventCodeAndDateAsync(normalizedCode, dto.Date))
                throw new BusinessException("SPECIAL_DAY_DUPLICATE", "Aynı tarih ve kod ile kayıt zaten mevcut.");

            var row = new SpecialDay
            {
                EventCode = normalizedCode,
                Name = normalizedName,
                Date = dto.Date,
                Category = dto.Category,
                Source = "manual",
                IsActive = dto.IsActive
            };
            row.SetCreated(adminId);
            await repository.AddAsync(row);
            return;
        }

        var existing = await repository.GetByIdAsync(dto.Id.Value)
            ?? throw new NotFoundException("Özel gün");

        if (!string.Equals(existing.Source, "manual", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("SPECIAL_DAY_SYSTEM_READ_ONLY", "Sistem günleri düzenlenemez.");

        if (await repository.ExistsByEventCodeAndDateAsync(normalizedCode, dto.Date, existing.Id))
            throw new BusinessException("SPECIAL_DAY_DUPLICATE", "Aynı tarih ve kod ile kayıt zaten mevcut.");

        existing.EventCode = normalizedCode;
        existing.Name = normalizedName;
        existing.Date = dto.Date;
        existing.Category = dto.Category;
        existing.IsActive = dto.IsActive;
        existing.SetUpdated(adminId);
        await repository.UpdateAsync(existing);
    }

    public async Task DeleteManualDayAsync(int id, int adminId)
    {
        var existing = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Özel gün");

        if (!string.Equals(existing.Source, "manual", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("SPECIAL_DAY_SYSTEM_READ_ONLY", "Sistem günleri silinemez.");

        existing.SetDeleted(adminId);
        await repository.DeleteAsync(existing);
    }

    private static List<SpecialDay> BuildNationalDays(int year)
    {
        return
        [
            Day("NEW_YEAR", "Yılbaşı", year, 1, 1, SpecialDayCategory.National),
            Day("NATIONAL_SOVEREIGNTY_AND_CHILDREN", "Ulusal Egemenlik ve Çocuk Bayramı", year, 4, 23, SpecialDayCategory.National),
            Day("LABOUR_DAY", "Emek ve Dayanışma Günü", year, 5, 1, SpecialDayCategory.National),
            Day("ATATURK_MEMORIAL_YOUTH_SPORTS", "Atatürk'ü Anma, Gençlik ve Spor Bayramı", year, 5, 19, SpecialDayCategory.National),
            Day("ATATURK_MEMORIAL_DAY", "Atatürk'ü Anma Günü", year, 11, 10, SpecialDayCategory.National),
            Day("DEMOCRACY_AND_NATIONAL_UNITY", "Demokrasi ve Millî Birlik Günü", year, 7, 15, SpecialDayCategory.National),
            Day("VICTORY_DAY", "Zafer Bayramı", year, 8, 30, SpecialDayCategory.National),
            Day("REPUBLIC_DAY", "Cumhuriyet Bayramı", year, 10, 29, SpecialDayCategory.National)
        ];
    }

    private static List<SpecialDay> BuildSpecialDays(int year)
    {
        var mothersDay = NthWeekdayOfMonth(year, 5, DayOfWeek.Sunday, 2);
        var fathersDay = NthWeekdayOfMonth(year, 6, DayOfWeek.Sunday, 3);

        return
        [
            Day("VALENTINES_DAY", "Sevgililer Günü", year, 2, 14, SpecialDayCategory.Special),
            Day("WOMENS_DAY", "Dünya Kadınlar Günü", year, 3, 8, SpecialDayCategory.Special),
            Day("MOTHERS_DAY", "Anneler Günü", mothersDay, SpecialDayCategory.Special),
            Day("FATHERS_DAY", "Babalar Günü", fathersDay, SpecialDayCategory.Special)
        ];
    }

    private async Task<List<SpecialDay>> BuildReligiousDaysAsync(int year)
    {
        var ramadanStart = (DateOnly?)null;
        var sacrificeStart = (DateOnly?)null;

        for (var month = 1; month <= 12; month++)
        {
            var url = $"https://api.aladhan.com/v1/gToHCalendar/{month}/{year}";
            try
            {
                var json = await Http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("data", out var dataElement) ||
                    dataElement.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var dayElement in dataElement.EnumerateArray())
                {
                    if (!TryParseGregorianDate(dayElement, out var gregDate))
                        continue;

                    if (!TryGetHijriInfo(dayElement, out var hijriMonth, out var hijriDay))
                        continue;

                    if (hijriMonth == 10 && hijriDay == 1)
                        ramadanStart = gregDate;

                    if (hijriMonth == 12 && hijriDay == 10)
                        sacrificeStart = gregDate;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Dini gün takvimi API çağrısı başarısız: Year={Year} Month={Month}", year, month);
            }
        }

        var result = new List<SpecialDay>();
        if (ramadanStart.HasValue)
        {
            result.Add(Day("RAMADAN_FEAST_DAY1", "Ramazan Bayramı 1. Gün", ramadanStart.Value, SpecialDayCategory.Religious));
            result.Add(Day("RAMADAN_FEAST_DAY2", "Ramazan Bayramı 2. Gün", ramadanStart.Value.AddDays(1), SpecialDayCategory.Religious));
            result.Add(Day("RAMADAN_FEAST_DAY3", "Ramazan Bayramı 3. Gün", ramadanStart.Value.AddDays(2), SpecialDayCategory.Religious));
        }

        if (sacrificeStart.HasValue)
        {
            result.Add(Day("SACRIFICE_FEAST_DAY1", "Kurban Bayramı 1. Gün", sacrificeStart.Value, SpecialDayCategory.Religious));
            result.Add(Day("SACRIFICE_FEAST_DAY2", "Kurban Bayramı 2. Gün", sacrificeStart.Value.AddDays(1), SpecialDayCategory.Religious));
            result.Add(Day("SACRIFICE_FEAST_DAY3", "Kurban Bayramı 3. Gün", sacrificeStart.Value.AddDays(2), SpecialDayCategory.Religious));
            result.Add(Day("SACRIFICE_FEAST_DAY4", "Kurban Bayramı 4. Gün", sacrificeStart.Value.AddDays(3), SpecialDayCategory.Religious));
        }

        return result;
    }

    private static bool TryParseGregorianDate(JsonElement dayElement, out DateOnly date)
    {
        date = default;
        if (!dayElement.TryGetProperty("gregorian", out var gregorianElement))
            return false;
        if (!gregorianElement.TryGetProperty("date", out var dateElement))
            return false;

        var raw = dateElement.GetString();
        return DateOnly.TryParseExact(raw, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static bool TryGetHijriInfo(JsonElement dayElement, out int month, out int day)
    {
        month = 0;
        day = 0;
        if (!dayElement.TryGetProperty("hijri", out var hijriElement))
            return false;
        if (!hijriElement.TryGetProperty("day", out var dayElementProp))
            return false;
        if (!hijriElement.TryGetProperty("month", out var monthElement))
            return false;
        if (!monthElement.TryGetProperty("number", out var monthNumberElement))
            return false;

        if (!int.TryParse(dayElementProp.GetString(), out day))
            return false;

        month = monthNumberElement.GetInt32();
        return true;
    }

    private static DateOnly NthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int nth)
    {
        var date = new DateOnly(year, month, 1);
        while (date.DayOfWeek != dayOfWeek)
            date = date.AddDays(1);

        return date.AddDays((nth - 1) * 7);
    }

    private static SpecialDay Day(
        string code,
        string name,
        int year,
        int month,
        int day,
        SpecialDayCategory category)
        => Day(code, name, new DateOnly(year, month, day), category);

    private static SpecialDay Day(
        string code,
        string name,
        DateOnly date,
        SpecialDayCategory category)
        => new()
        {
            EventCode = code,
            Name = name,
            Date = date,
            Category = category,
            Source = "system",
            IsActive = true
        };

    private static SpecialDayDto Map(SpecialDay x)
        => new(
            x.Id,
            x.EventCode,
            x.Name,
            x.Date,
            x.Category,
            x.Source,
            x.IsActive);

    private static void ValidateManual(SaveSpecialDayDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EventCode))
            throw new BusinessException("SPECIAL_DAY_CODE_REQUIRED", "Kod zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("SPECIAL_DAY_NAME_REQUIRED", "Gün adı zorunludur.");
    }
}
