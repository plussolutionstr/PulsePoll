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

    public async Task<ProjectPerformanceResultDto> GetProjectPerformanceAsync()
    {
        var projects = await db.Projects
            .Include(p => p.Customer)
            .AsNoTracking()
            .ToListAsync();

        var assignments = await db.ProjectAssignments
            .GroupBy(a => new { a.ProjectId, a.Status })
            .Select(g => new { g.Key.ProjectId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var earnedByProject = await db.ProjectAssignments
            .Where(a => a.EarnedAmount > 0)
            .GroupBy(a => a.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Sum(a => a.EarnedAmount ?? 0) })
            .ToListAsync();

        var earnedMap = earnedByProject.ToDictionary(x => x.ProjectId, x => x.Total);

        var items = projects.Select(p =>
        {
            var pAssignments = assignments.Where(a => a.ProjectId == p.Id).ToList();
            var total = pAssignments.Sum(a => a.Count);
            var completed = pAssignments.Where(a => a.Status == AssignmentStatus.Completed).Sum(a => a.Count);
            var notStarted = pAssignments.Where(a => a.Status == AssignmentStatus.NotStarted || a.Status == AssignmentStatus.Scheduled).Sum(a => a.Count);
            var partial = pAssignments.Where(a => a.Status == AssignmentStatus.Partial).Sum(a => a.Count);
            var disqualify = pAssignments.Where(a => a.Status == AssignmentStatus.Disqualify).Sum(a => a.Count);
            var screenOut = pAssignments.Where(a => a.Status == AssignmentStatus.ScreenOut).Sum(a => a.Count);
            var quotaFull = pAssignments.Where(a => a.Status == AssignmentStatus.QuotaFull).Sum(a => a.Count);

            return new ProjectPerformanceItemDto
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                CustomerName = p.Customer?.Title ?? "-",
                Status = p.Status.GetDescription(),
                TotalAssigned = total,
                Completed = completed,
                NotStarted = notStarted,
                Partial = partial,
                Disqualify = disqualify,
                ScreenOut = screenOut,
                QuotaFull = quotaFull,
                CompletionRate = total > 0 ? Math.Round((decimal)completed / total * 100, 1) : 0,
                Reward = p.Reward,
                TotalDistributed = earnedMap.GetValueOrDefault(p.Id, 0)
            };
        })
        .OrderByDescending(x => x.TotalAssigned)
        .ToList();

        var totalAssigned = items.Sum(i => i.TotalAssigned);
        var totalCompleted = items.Sum(i => i.Completed);

        return new ProjectPerformanceResultDto
        {
            Summary = new ProjectPerformanceSummaryDto
            {
                TotalProjects = items.Count,
                ActiveProjects = projects.Count(p => p.Status == ProjectStatus.Active),
                CompletedProjects = projects.Count(p => p.Status == ProjectStatus.Completed),
                TotalAssigned = totalAssigned,
                TotalCompleted = totalCompleted,
                OverallCompletionRate = totalAssigned > 0 ? Math.Round((decimal)totalCompleted / totalAssigned * 100, 1) : 0,
                TotalDistributed = items.Sum(i => i.TotalDistributed)
            },
            Items = items
        };
    }

    public async Task<CustomerReportResultDto> GetCustomerReportAsync()
    {
        var customers = await db.Customers
            .AsNoTracking()
            .ToListAsync();

        var projectData = await db.Projects
            .Select(p => new { p.Id, p.CustomerId, p.Status, p.Budget })
            .AsNoTracking()
            .ToListAsync();

        var assignmentData = await db.ProjectAssignments
            .GroupBy(a => new { a.ProjectId, a.Status })
            .Select(g => new { g.Key.ProjectId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var earnedByProject = await db.ProjectAssignments
            .Where(a => a.EarnedAmount > 0)
            .GroupBy(a => a.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Sum(a => a.EarnedAmount ?? 0) })
            .ToListAsync();

        var earnedMap = earnedByProject.ToDictionary(x => x.ProjectId, x => x.Total);

        var items = customers.Select(c =>
        {
            var cProjects = projectData.Where(p => p.CustomerId == c.Id).ToList();
            var cProjectIds = cProjects.Select(p => p.Id).ToHashSet();
            var cAssignments = assignmentData.Where(a => cProjectIds.Contains(a.ProjectId)).ToList();

            var totalAssigned = cAssignments.Sum(a => a.Count);
            var totalCompleted = cAssignments.Where(a => a.Status == AssignmentStatus.Completed).Sum(a => a.Count);
            var totalDistributed = cProjectIds.Sum(id => earnedMap.GetValueOrDefault(id, 0));

            return new CustomerReportItemDto
            {
                CustomerId = c.Id,
                CustomerName = c.Title,
                Code = c.Code,
                ProjectCount = cProjects.Count,
                ActiveProjects = cProjects.Count(p => p.Status == ProjectStatus.Active),
                CompletedProjects = cProjects.Count(p => p.Status == ProjectStatus.Completed),
                TotalAssigned = totalAssigned,
                TotalCompleted = totalCompleted,
                CompletionRate = totalAssigned > 0 ? Math.Round((decimal)totalCompleted / totalAssigned * 100, 1) : 0,
                TotalBudget = cProjects.Sum(p => p.Budget),
                TotalDistributed = totalDistributed
            };
        })
        .OrderByDescending(x => x.ProjectCount)
        .ToList();

        return new CustomerReportResultDto
        {
            Summary = new CustomerReportSummaryDto
            {
                TotalCustomers = items.Count,
                TotalProjects = projectData.Count,
                TotalBudget = items.Sum(i => i.TotalBudget),
                TotalDistributed = items.Sum(i => i.TotalDistributed)
            },
            Items = items
        };
    }

    public async Task<PaymentReportResultDto> GetPaymentReportAsync()
    {
        var batches = await db.PaymentBatches
            .Select(b => new { b.Status, b.TotalAmount })
            .AsNoTracking()
            .ToListAsync();

        var withdrawals = await db.WithdrawalRequests
            .Select(w => new { w.Status, w.AmountTry })
            .AsNoTracking()
            .ToListAsync();

        var bankDist = await db.WithdrawalRequests
            .Where(w => w.Status == ApprovalStatus.Approved)
            .Include(w => w.BankAccount)
            .GroupBy(w => w.BankAccount!.BankName)
            .Select(g => new PaymentBankDistributionDto
            {
                BankName = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(w => w.AmountTry)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();

        return new PaymentReportResultDto
        {
            BatchSummary = new PaymentReportBatchSummaryDto
            {
                DraftCount = batches.Count(b => b.Status == PaymentBatchStatus.Draft),
                SentCount = batches.Count(b => b.Status == PaymentBatchStatus.Sent),
                CompletedCount = batches.Count(b => b.Status == PaymentBatchStatus.Completed),
                DraftAmount = batches.Where(b => b.Status == PaymentBatchStatus.Draft).Sum(b => b.TotalAmount),
                SentAmount = batches.Where(b => b.Status == PaymentBatchStatus.Sent).Sum(b => b.TotalAmount),
                CompletedAmount = batches.Where(b => b.Status == PaymentBatchStatus.Completed).Sum(b => b.TotalAmount)
            },
            WithdrawalSummary = new PaymentReportWithdrawalSummaryDto
            {
                PendingCount = withdrawals.Count(w => w.Status == ApprovalStatus.Pending),
                ApprovedCount = withdrawals.Count(w => w.Status == ApprovalStatus.Approved),
                RejectedCount = withdrawals.Count(w => w.Status == ApprovalStatus.Rejected),
                PendingAmount = withdrawals.Where(w => w.Status == ApprovalStatus.Pending).Sum(w => w.AmountTry),
                ApprovedAmount = withdrawals.Where(w => w.Status == ApprovalStatus.Approved).Sum(w => w.AmountTry),
                RejectedAmount = withdrawals.Where(w => w.Status == ApprovalStatus.Rejected).Sum(w => w.AmountTry)
            },
            BankDistribution = bankDist
        };
    }

    public async Task<SmsReportResultDto> GetSmsReportAsync()
    {
        var smsData = await db.SmsLogs
            .GroupBy(s => new { s.Source, s.DeliveryStatus })
            .Select(g => new { g.Key.Source, g.Key.DeliveryStatus, Count = g.Count() })
            .ToListAsync();

        var bySource = smsData
            .GroupBy(x => x.Source)
            .Select(g => new SmsReportBySourceDto
            {
                Source = g.Key.GetDescription(),
                TotalCount = g.Sum(x => x.Count),
                SentCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Sent).Sum(x => x.Count),
                FailedCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Failed).Sum(x => x.Count),
                PendingCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Pending).Sum(x => x.Count),
                SkippedCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Skipped).Sum(x => x.Count)
            })
            .OrderByDescending(x => x.TotalCount)
            .ToList();

        var total = bySource.Sum(x => x.TotalCount);
        var sent = bySource.Sum(x => x.SentCount);

        return new SmsReportResultDto
        {
            Summary = new SmsReportSummaryDto
            {
                TotalCount = total,
                SentCount = sent,
                FailedCount = bySource.Sum(x => x.FailedCount),
                PendingCount = bySource.Sum(x => x.PendingCount),
                SkippedCount = bySource.Sum(x => x.SkippedCount),
                SuccessRate = total > 0 ? Math.Round((decimal)sent / total * 100, 1) : 0
            },
            BySource = bySource
        };
    }

    public async Task<NotificationReportResultDto> GetNotificationReportAsync()
    {
        var rawData = await db.Notifications
            .GroupBy(n => new { Type = n.Type ?? "Genel", n.DeliveryStatus, n.IsRead })
            .Select(g => new
            {
                g.Key.Type,
                g.Key.DeliveryStatus,
                g.Key.IsRead,
                Count = g.Count()
            })
            .ToListAsync();

        var byType = rawData
            .GroupBy(x => x.Type)
            .Select(g => new NotificationReportByTypeDto
            {
                Type = g.Key,
                TotalCount = g.Sum(x => x.Count),
                SentCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Sent).Sum(x => x.Count),
                FailedCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Failed).Sum(x => x.Count),
                PendingCount = g.Where(x => x.DeliveryStatus == DeliveryStatus.Pending).Sum(x => x.Count),
                ReadCount = g.Where(x => x.IsRead).Sum(x => x.Count),
                UnreadCount = g.Where(x => !x.IsRead).Sum(x => x.Count)
            })
            .OrderByDescending(x => x.TotalCount)
            .ToList();

        var total = byType.Sum(x => x.TotalCount);
        var sent = byType.Sum(x => x.SentCount);
        var read = byType.Sum(x => x.ReadCount);

        return new NotificationReportResultDto
        {
            Summary = new NotificationReportSummaryDto
            {
                TotalCount = total,
                SentCount = sent,
                FailedCount = byType.Sum(x => x.FailedCount),
                PendingCount = byType.Sum(x => x.PendingCount),
                ReadCount = read,
                UnreadCount = byType.Sum(x => x.UnreadCount),
                ReadRate = total > 0 ? Math.Round((decimal)read / total * 100, 1) : 0,
                DeliveryRate = total > 0 ? Math.Round((decimal)sent / total * 100, 1) : 0
            },
            ByType = byType
        };
    }

    private static double CalcChangePercent(int current, int previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        return Math.Round((double)(current - previous) / previous * 100, 1);
    }
}
