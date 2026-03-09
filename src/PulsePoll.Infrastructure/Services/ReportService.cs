using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Extensions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Enums;
using PulsePoll.Infrastructure.Persistence;

namespace PulsePoll.Infrastructure.Services;

public class ReportService(AppDbContext db) : IReportService
{
    public async Task<SubjectRoadmapResultDto> GetSubjectRoadmapAsync(int year)
    {
        // Her deneğin ilk app activity kaydından platform bilgisini al
        var firstPlatformBySubject = db.SubjectAppActivities
            .GroupBy(a => a.SubjectId)
            .Select(g => new
            {
                SubjectId = g.Key,
                Platform = g.OrderBy(a => a.OccurredAt).First().Platform
            });

        // Onaylı denekleri ay ve platform bazında grupla
        var rawData = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Where(s => s.CreatedAt.Year == year)
            .GroupJoin(
                firstPlatformBySubject,
                s => s.Id,
                p => p.SubjectId,
                (s, platforms) => new { s.CreatedAt, Platform = platforms.Select(p => p.Platform).FirstOrDefault() })
            .GroupBy(x => new { x.CreatedAt.Month, x.Platform })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.Platform,
                Count = g.Count()
            })
            .ToListAsync();

        var months = Enumerable.Range(1, 12).Select(m =>
        {
            var iosCount = rawData
                .Where(r => r.Month == m && r.Platform != null &&
                            r.Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Count);
            var androidCount = rawData
                .Where(r => r.Month == m && r.Platform != null &&
                            r.Platform.Equals("Android", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Count);
            var unknownCount = rawData
                .Where(r => r.Month == m &&
                            (r.Platform == null ||
                             (!r.Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase) &&
                              !r.Platform.Equals("Android", StringComparison.OrdinalIgnoreCase))))
                .Sum(r => r.Count);

            // Bilinmeyen platformları toplama dahil et
            var total = iosCount + androidCount + unknownCount;

            return new SubjectRoadmapMonthDto
            {
                Year = year,
                Month = m,
                IosCount = iosCount,
                AndroidCount = androidCount,
                TotalCount = total
            };
        }).ToList();

        var totalIos = months.Sum(m => m.IosCount);
        var totalAndroid = months.Sum(m => m.AndroidCount);
        var grandTotal = months.Sum(m => m.TotalCount);

        // Önceki yıl karşılaştırma
        var prevYear = year - 1;
        var prevYearCounts = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Where(s => s.CreatedAt.Year == prevYear)
            .GroupJoin(
                firstPlatformBySubject,
                s => s.Id,
                p => p.SubjectId,
                (s, platforms) => new { Platform = platforms.Select(p => p.Platform).FirstOrDefault() })
            .GroupBy(x => x.Platform)
            .Select(g => new
            {
                Platform = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var prevIos = prevYearCounts
            .Where(p => p.Platform != null && p.Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Count);
        var prevAndroid = prevYearCounts
            .Where(p => p.Platform != null && p.Platform.Equals("Android", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Count);
        var prevTotal = prevYearCounts.Sum(p => p.Count);

        return new SubjectRoadmapResultDto
        {
            Year = year,
            Months = months,
            TotalIos = totalIos,
            TotalAndroid = totalAndroid,
            GrandTotal = grandTotal,
            ChangePercent = CalcChangePercent(grandTotal, prevTotal),
            IosChangePercent = CalcChangePercent(totalIos, prevIos),
            AndroidChangePercent = CalcChangePercent(totalAndroid, prevAndroid)
        };
    }

    public async Task<List<int>> GetAvailableYearsAsync()
    {
        var years = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Select(s => s.CreatedAt.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        // Mevcut yılı ekle (henüz kayıt yoksa bile)
        var currentYear = TurkeyTime.Now.Year;
        if (!years.Contains(currentYear))
            years.Insert(0, currentYear);

        return years;
    }

    public async Task<SubjectActivityResultDto> GetSubjectActivityAsync(int days)
    {
        var cutoff = TurkeyTime.Now.AddDays(-days);

        // Onaylı tüm denekler
        var approvedSubjects = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Select(s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.PhoneNumber
            })
            .AsNoTracking()
            .ToListAsync();

        var subjectIds = approvedSubjects.Select(s => s.Id).ToList();

        // Her denek için: son görülme, platform, seçilen süredeki aktif gün sayısı
        var activityData = await db.SubjectAppActivities
            .Where(a => subjectIds.Contains(a.SubjectId))
            .GroupBy(a => a.SubjectId)
            .Select(g => new
            {
                SubjectId = g.Key,
                LastSeenAt = g.Max(a => a.OccurredAt),
                Platform = g.OrderByDescending(a => a.OccurredAt).First().Platform,
                ActiveDays = g.Count(a => a.OccurredAt >= cutoff)
            })
            .ToListAsync();

        var activityMap = activityData.ToDictionary(a => a.SubjectId);

        var items = approvedSubjects.Select(s =>
        {
            var hasActivity = activityMap.TryGetValue(s.Id, out var activity);

            SubjectActivityStatus status;
            if (!hasActivity)
                status = SubjectActivityStatus.NeverLoggedIn;
            else if (activity!.LastSeenAt >= cutoff)
                status = SubjectActivityStatus.Active;
            else
                status = SubjectActivityStatus.Passive;

            return new SubjectActivityItemDto
            {
                SubjectId = s.Id,
                FullName = $"{s.FirstName} {s.LastName}",
                PhoneNumber = s.PhoneNumber,
                Platform = activity?.Platform,
                LastSeenAt = activity?.LastSeenAt,
                ActiveDays = activity?.ActiveDays ?? 0,
                Status = status
            };
        })
        .OrderByDescending(x => x.LastSeenAt ?? DateTime.MinValue)
        .ToList();

        var activeCount = items.Count(i => i.Status == SubjectActivityStatus.Active);
        var passiveCount = items.Count(i => i.Status == SubjectActivityStatus.Passive);
        var neverCount = items.Count(i => i.Status == SubjectActivityStatus.NeverLoggedIn);

        return new SubjectActivityResultDto
        {
            Summary = new SubjectActivitySummaryDto
            {
                ActiveCount = activeCount,
                PassiveCount = passiveCount,
                NeverLoggedInCount = neverCount,
                TotalApproved = approvedSubjects.Count
            },
            Items = items
        };
    }

    public async Task<SubjectDemographicsResultDto> GetSubjectDemographicsAsync()
    {
        var subjects = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Include(s => s.City)
            .Include(s => s.Profession)
            .Include(s => s.EducationLevel)
            .Include(s => s.SocioeconomicStatus)
            .AsNoTracking()
            .ToListAsync();

        var totalFemale = subjects.Count(s => s.Gender == Gender.Female);
        var totalMale = subjects.Count(s => s.Gender == Gender.Male);

        var sections = new List<DemographicSection>
        {
            BuildSection("SES", subjects, s => s.SocioeconomicStatus?.Code ?? "Bilinmiyor",
                s => s.SocioeconomicStatus?.Code ?? "Bilinmiyor"),
            BuildSection("Medeni Durum", subjects, s => s.MaritalStatus.GetDescription(),
                s => ((int)s.MaritalStatus).ToString()),
            BuildSection("GSM Operatörü", subjects, s => s.GsmOperator.GetDescription(),
                s => ((int)s.GsmOperator).ToString()),
            BuildSection("Meslek", subjects, s => s.Profession?.Name ?? "Bilinmiyor",
                s => s.Profession?.Name ?? "Bilinmiyor"),
            BuildSection("Eğitim Durumu", subjects, s => s.EducationLevel?.Name ?? "Bilinmiyor",
                s => s.EducationLevel?.OrderIndex.ToString() ?? "0"),
            BuildSection("Şehir", subjects, s => s.City?.Name ?? "Bilinmiyor",
                s => s.City?.Name ?? "Bilinmiyor"),
            BuildSection("Coğrafi Bölge", subjects, s => s.City?.Region ?? "Bilinmiyor",
                s => s.City?.Region ?? "Bilinmiyor"),
            BuildSection("NUTS 1 Coğrafi Bölge", subjects, s => s.City?.Nuts1Region ?? "Bilinmiyor",
                s => s.City?.Nuts1Region ?? "Bilinmiyor"),
        };

        return new SubjectDemographicsResultDto
        {
            Sections = sections,
            TotalFemale = totalFemale,
            TotalMale = totalMale,
            GrandTotal = subjects.Count
        };
    }

    private static DemographicSection BuildSection(
        string groupName,
        List<Domain.Entities.Subject> subjects,
        Func<Domain.Entities.Subject, string> valueSelector,
        Func<Domain.Entities.Subject, string> sortKeySelector)
    {
        var rows = subjects
            .GroupBy(valueSelector)
            .Select(g => new DemographicRow
            {
                Group = groupName,
                Value = g.Key,
                FemaleCount = g.Count(s => s.Gender == Gender.Female),
                MaleCount = g.Count(s => s.Gender == Gender.Male),
                TotalCount = g.Count()
            })
            .OrderByDescending(r => r.TotalCount)
            .ToList();

        return new DemographicSection
        {
            GroupName = groupName,
            Rows = rows,
            TotalFemale = rows.Sum(r => r.FemaleCount),
            TotalMale = rows.Sum(r => r.MaleCount),
            GrandTotal = rows.Sum(r => r.TotalCount)
        };
    }

    public async Task<SubjectEarningsResultDto> GetSubjectEarningsAsync()
    {
        var subjects = await db.Subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.PhoneNumber })
            .AsNoTracking()
            .ToListAsync();

        var subjectIds = subjects.Select(s => s.Id).ToList();

        var wallets = await db.Wallets
            .Where(w => subjectIds.Contains(w.SubjectId))
            .Select(w => new { w.SubjectId, w.Balance })
            .AsNoTracking()
            .ToListAsync();

        // Tüm hesaplamaları transaction'lardan yap (TotalEarned güvenilir değil)
        var transactionTotals = await db.WalletTransactions
            .Where(t => t.Wallet != null && subjectIds.Contains(t.Wallet.SubjectId))
            .GroupBy(t => new { t.Wallet!.SubjectId, t.Type })
            .Select(g => new { g.Key.SubjectId, g.Key.Type, Total = g.Sum(t => t.Amount) })
            .ToListAsync();

        var creditMap = transactionTotals
            .Where(t => t.Type == WalletTransactionType.Credit)
            .ToDictionary(t => t.SubjectId, t => t.Total);
        var withdrawalMap = transactionTotals
            .Where(t => t.Type == WalletTransactionType.Withdrawal)
            .ToDictionary(t => t.SubjectId, t => t.Total);

        var walletMap = wallets.ToDictionary(w => w.SubjectId);

        var items = subjects.Select(s =>
        {
            var hasWallet = walletMap.TryGetValue(s.Id, out var wallet);
            var totalEarned = creditMap.GetValueOrDefault(s.Id, 0);
            var totalWithdrawn = withdrawalMap.GetValueOrDefault(s.Id, 0);

            return new SubjectEarningsItemDto
            {
                SubjectId = s.Id,
                FullName = $"{s.FirstName} {s.LastName}",
                PhoneNumber = s.PhoneNumber,
                TotalEarned = totalEarned,
                TotalWithdrawn = Math.Abs(totalWithdrawn),
                Balance = hasWallet ? wallet!.Balance : 0
            };
        })
        .OrderByDescending(x => x.TotalEarned)
        .ToList();

        return new SubjectEarningsResultDto
        {
            Summary = new SubjectEarningsSummaryDto
            {
                TotalEarned = items.Sum(i => i.TotalEarned),
                TotalWithdrawn = items.Sum(i => i.TotalWithdrawn),
                TotalBalance = items.Sum(i => i.Balance),
                SubjectCount = items.Count
            },
            Items = items
        };
    }

    private static double CalcChangePercent(int current, int previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        return Math.Round((double)(current - previous) / previous * 100, 1);
    }
}
