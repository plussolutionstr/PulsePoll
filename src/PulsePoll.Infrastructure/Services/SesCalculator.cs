using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Infrastructure.Persistence;

namespace PulsePoll.Infrastructure.Services;

/// <summary>
/// TÜAD 2012 SES (Sosyo-Ekonomik Statü) Hesaplayıcı
/// Kaynak: TÜAD SES Projesi 2012
/// </summary>
public class SesCalculator(AppDbContext db) : ISesCalculator
{
    private Dictionary<int, string>? _professionCodes;
    private Dictionary<int, int>? _educationOrders;
    private Dictionary<string, int>? _sesCodes;

    public async Task<int?> CalculateSesIdAsync(
        int? professionId,
        int? educationLevelId,
        bool isRetired,
        bool isHeadOfFamily,
        int? headOfFamilyProfessionId = null,
        int? headOfFamilyEducationLevelId = null,
        bool isHeadOfFamilyRetired = false)
    {
        await EnsureCachesLoaded();

        var professionCode = professionId.HasValue && _professionCodes!.TryGetValue(professionId.Value, out var pc) ? pc : null;
        var educationOrder = educationLevelId.HasValue && _educationOrders!.TryGetValue(educationLevelId.Value, out var eo) ? eo : (int?)null;
        var hofProfessionCode = headOfFamilyProfessionId.HasValue && _professionCodes!.TryGetValue(headOfFamilyProfessionId.Value, out var hpc) ? hpc : null;
        var hofEducationOrder = headOfFamilyEducationLevelId.HasValue && _educationOrders!.TryGetValue(headOfFamilyEducationLevelId.Value, out var heo) ? heo : (int?)null;

        var sesCode = CalculateSes(
            professionCode, educationOrder, isRetired, isHeadOfFamily,
            hofProfessionCode, hofEducationOrder, isHeadOfFamilyRetired);

        if (sesCode != null && _sesCodes!.TryGetValue(sesCode, out var sesId))
            return sesId;

        return null;
    }

    private async Task EnsureCachesLoaded()
    {
        _professionCodes ??= await db.Professions
            .Where(p => p.Code != null)
            .ToDictionaryAsync(p => p.Id, p => p.Code!);

        _educationOrders ??= await db.EducationLevels
            .ToDictionaryAsync(e => e.Id, e => e.OrderIndex);

        _sesCodes ??= await db.SocioeconomicStatuses
            .Where(s => s.Code != null)
            .ToDictionaryAsync(s => s.Code!, s => s.Id);
    }

    private static string? CalculateSes(
        string? professionCode,
        int? educationOrder,
        bool isRetired,
        bool isHeadOfFamily,
        string? hofProfessionCode,
        int? hofEducationOrder,
        bool hofIsRetired)
    {
        if (isHeadOfFamily)
            return GetSesFromTable(professionCode, educationOrder, isRetired);

        if (hofProfessionCode != null && hofEducationOrder.HasValue)
            return GetSesFromTable(hofProfessionCode, hofEducationOrder, hofIsRetired);

        // Hane reisi bilgisi eksik — fallback: kendi bilgileri
        return GetSesFromTable(professionCode, educationOrder, isRetired);
    }

    private static string? GetSesFromTable(string? professionCode, int? educationOrder, bool isRetired)
    {
        if (string.IsNullOrEmpty(professionCode) || !educationOrder.HasValue)
            return null;

        if (educationOrder < 1 || educationOrder > 9)
            return null;

        var mapping = GetProfessionMapping(professionCode);
        if (mapping is null)
            return null;

        var (meslek, kisim) = mapping.Value;
        int eduIdx = educationOrder.Value - 1; // 0-based

        bool isRetiredWorking = professionCode == "EMKC";

        if (kisim == "RETIRED")
        {
            return isRetiredWorking
                ? SesTableB["4b"][eduIdx]
                : SesTableRetiredNotWorkingB["4b"][eduIdx];
        }

        var table = kisim switch
        {
            "A" => SesTableA,
            "B" => (isRetired && !isRetiredWorking) ? SesTableRetiredNotWorkingB : SesTableB,
            "C" => (isRetired && !isRetiredWorking) ? SesTableRetiredNotWorkingC : SesTableC,
            _ => null
        };

        if (table is null || !table.TryGetValue(meslek, out var row))
            return null;

        var ses = row[eduIdx];

        // null gelirse varsayılan ata
        if (ses is null)
        {
            if (meslek == "3" && eduIdx < 2) ses = "D";
            else if (meslek == "11" && eduIdx == 0) ses = "C2";
            else if ((meslek == "12" || meslek == "20") && eduIdx < 5) ses = "B";
        }

        return ses;
    }

    private static (string meslek, string kisim)? GetProfessionMapping(string code) => code switch
    {
        "ISSZ" => ("1a", "A"),
        "EVHN" => ("2a", "A"),
        "OGRN" => ("3",  "A"),
        "YVML" => ("4a", "B"),
        "OZCL" => ("4b", "B"),
        "KMCL" => ("4b", "B"),
        "OZSU" => ("6",  "B"),
        "KMSU" => ("6",  "B"),
        "OZYN" => ("8",  "B"),
        "KMYN" => ("8",  "B"),
        "TSKP" => ("11", "B"),
        "DKTR" => ("12", "B"),
        "AVKT" => ("12", "B"),
        "MMHD" => ("12", "B"),
        "AKDM" => ("12", "B"),
        "CFTC" => ("13", "C"),
        "FRLC" => ("14", "C"),
        "ESNF" => ("15", "C"),
        "SRKS" => ("17", "C"),
        "ECZC" => ("20", "C"),
        "MLMS" => ("20", "C"),
        "EMKL" => ("retired_not_working", "RETIRED"),
        "EMKC" => ("retired_working",     "RETIRED"),
        "DIGR" => ("4b", "B"),
        _      => ("4b", "B")
    };

    #region SES Tabloları

    private static readonly Dictionary<string, string?[]> SesTableA = new()
    {
        ["1a"] = ["E",  "E",  "D",  "D",  "D",  "C2", "C2", "C2", "C2"],
        ["1b"] = ["D",  "D",  "C2", "C2", "C2", "C1", "C1", "C1", "C1"],
        ["2a"] = ["E",  "E",  "D",  "D",  "D",  "C2", "C2", "C2", "C2"],
        ["2b"] = ["D",  "D",  "C2", "C2", "C2", "C1", "C1", "C1", "C1"],
        ["3"]  = [null, null, "D",  "C2", "C2", "C2", "C2", "C2", "C2"],
    };

    private static readonly Dictionary<string, string?[]> SesTableB = new()
    {
        ["4a"] = ["E",  "D",  "D",  "C2", "C2", "C2", "C1", "C1", "B"],
        ["4b"] = ["D",  "C2", "C2", "C1", "C1", "C1", "B",  "B",  "B"],
        ["5"]  = ["D",  "C2", "C2", "C1", "C1", "B",  "B",  "B",  "B"],
        ["6"]  = ["D",  "C2", "C2", "C1", "C1", "B",  "B",  "B",  "B"],
        ["7"]  = ["C2", "C1", "C1", "C1", "C1", "B",  "B",  "A",  "A"],
        ["8"]  = ["C2", "C1", "C1", "C1", "B",  "B",  "B",  "A",  "A"],
        ["9"]  = ["C1", "C1", "C1", "B",  "B",  "B",  "A",  "A",  "A"],
        ["10"] = ["C1", "C1", "B",  "B",  "B",  "A",  "A",  "A",  "A"],
        ["11"] = [null, "C2", "C2", "C1", "C1", "B",  "B",  "A",  "A"],
        ["12"] = [null, null, null, null, null, "A",  "A",  "A",  "A"],
    };

    private static readonly Dictionary<string, string?[]> SesTableC = new()
    {
        ["13"] = ["D",  "D",  "D",  "C2", "C2", "C2", "C1", "B",  "B"],
        ["14"] = ["C2", "C2", "C2", "C1", "C1", "C1", "B",  "B",  "B"],
        ["15"] = ["C2", "C1", "C1", "C1", "C1", "B",  "B",  "A",  "A"],
        ["16"] = ["C2", "C1", "C1", "B",  "B",  "B",  "B",  "A",  "A"],
        ["17"] = ["C1", "C1", "C1", "B",  "B",  "B",  "A",  "A",  "A"],
        ["18"] = ["C1", "C1", "B",  "B",  "B",  "B",  "A",  "A",  "A"],
        ["19"] = ["C1", "C1", "B",  "B",  "B",  "A",  "A",  "A",  "A"],
        ["20"] = [null, null, null, null, null, "A",  "A",  "A",  "A"],
    };

    private static readonly Dictionary<string, string?[]> SesTableRetiredNotWorkingB = new()
    {
        ["4a"] = ["E",  "E",  "E",  "D",  "D",  "D",  "C2", "C2", "C1"],
        ["4b"] = ["E",  "D",  "D",  "C2", "C2", "C2", "C1", "C1", "C1"],
        ["5"]  = ["E",  "D",  "D",  "C2", "C2", "C1", "C1", "C1", "C1"],
        ["6"]  = ["E",  "D",  "D",  "C2", "C2", "C1", "C1", "C1", "C1"],
        ["7"]  = ["D",  "C2", "C2", "C2", "C2", "C1", "C1", "B",  "B"],
        ["8"]  = ["D",  "C2", "C2", "C2", "C1", "C1", "C1", "B",  "B"],
        ["9"]  = ["C2", "C2", "C2", "C1", "C1", "C1", "B",  "B",  "B"],
        ["10"] = ["C2", "C2", "C1", "C1", "C1", "B",  "B",  "B",  "B"],
        ["11"] = [null, "D",  "D",  "C2", "C2", "C1", "C1", "B",  "B"],
        ["12"] = [null, null, null, null, null, "B",  "B",  "B",  "B"],
    };

    private static readonly Dictionary<string, string?[]> SesTableRetiredNotWorkingC = new()
    {
        ["13"] = ["E",  "E",  "E",  "D",  "D",  "D",  "C2", "C1", "C1"],
        ["14"] = ["D",  "D",  "D",  "C2", "C2", "C2", "C1", "C1", "C1"],
        ["15"] = ["D",  "C2", "C2", "C2", "C2", "C1", "C1", "B",  "B"],
        ["16"] = ["D",  "C2", "C2", "C1", "C1", "C1", "C1", "B",  "B"],
        ["17"] = ["C2", "C2", "C2", "C1", "C1", "C1", "B",  "B",  "B"],
        ["18"] = ["C2", "C2", "C1", "C1", "C1", "C1", "B",  "B",  "B"],
        ["19"] = ["C2", "C2", "C1", "C1", "C1", "B",  "B",  "B",  "B"],
        ["20"] = [null, null, null, null, null, "B",  "B",  "B",  "B"],
    };

    #endregion
}
