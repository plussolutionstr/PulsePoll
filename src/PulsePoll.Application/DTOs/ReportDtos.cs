namespace PulsePoll.Application.DTOs;

public record SubjectRoadmapMonthDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int IosCount { get; init; }
    public int AndroidCount { get; init; }
    public int TotalCount { get; init; }
}

public record SubjectRoadmapResultDto
{
    public int Year { get; init; }
    public List<SubjectRoadmapMonthDto> Months { get; init; } = [];
    public int TotalIos { get; init; }
    public int TotalAndroid { get; init; }
    public int GrandTotal { get; init; }
    public double ChangePercent { get; init; }
    public double IosChangePercent { get; init; }
    public double AndroidChangePercent { get; init; }
}

public enum SubjectActivityStatus
{
    Active,
    Passive,
    NeverLoggedIn
}

public record SubjectActivitySummaryDto
{
    public int ActiveCount { get; init; }
    public int PassiveCount { get; init; }
    public int NeverLoggedInCount { get; init; }
    public int TotalApproved { get; init; }
}

public record SubjectActivityItemDto
{
    public int SubjectId { get; init; }
    public string FullName { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public string? Platform { get; init; }
    public DateTime? LastSeenAt { get; init; }
    public int ActiveDays { get; init; }
    public SubjectActivityStatus Status { get; init; }
}

public record SubjectActivityResultDto
{
    public SubjectActivitySummaryDto Summary { get; init; } = new();
    public List<SubjectActivityItemDto> Items { get; init; } = [];
}

public record DemographicRow
{
    public string Group { get; init; } = "";
    public string Value { get; init; } = "";
    public int FemaleCount { get; init; }
    public int MaleCount { get; init; }
    public int TotalCount { get; init; }
}

public record DemographicSection
{
    public string GroupName { get; init; } = "";
    public List<DemographicRow> Rows { get; init; } = [];
    public int TotalFemale { get; init; }
    public int TotalMale { get; init; }
    public int GrandTotal { get; init; }
}

public record SubjectDemographicsResultDto
{
    public List<DemographicSection> Sections { get; init; } = [];
    public int TotalFemale { get; init; }
    public int TotalMale { get; init; }
    public int GrandTotal { get; init; }
}

public record SubjectEarningsItemDto
{
    public int SubjectId { get; init; }
    public string FullName { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public decimal TotalEarned { get; init; }
    public decimal TotalWithdrawn { get; init; }
    public decimal Balance { get; init; }
}

public record SubjectEarningsSummaryDto
{
    public decimal TotalEarned { get; init; }
    public decimal TotalWithdrawn { get; init; }
    public decimal TotalBalance { get; init; }
    public int SubjectCount { get; init; }
}

public record SubjectEarningsResultDto
{
    public SubjectEarningsSummaryDto Summary { get; init; } = new();
    public List<SubjectEarningsItemDto> Items { get; init; } = [];
}

// ── Anket Performans Raporu ──

public record ProjectPerformanceItemDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = "";
    public string CustomerName { get; init; } = "";
    public string Status { get; init; } = "";
    public int TotalAssigned { get; init; }
    public int Completed { get; init; }
    public int NotStarted { get; init; }
    public int Partial { get; init; }
    public int Disqualify { get; init; }
    public int ScreenOut { get; init; }
    public int QuotaFull { get; init; }
    public decimal CompletionRate { get; init; }
    public decimal Reward { get; init; }
    public decimal TotalDistributed { get; init; }
}

public record ProjectPerformanceSummaryDto
{
    public int TotalProjects { get; init; }
    public int ActiveProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int TotalAssigned { get; init; }
    public int TotalCompleted { get; init; }
    public decimal OverallCompletionRate { get; init; }
    public decimal TotalDistributed { get; init; }
}

public record ProjectPerformanceResultDto
{
    public ProjectPerformanceSummaryDto Summary { get; init; } = new();
    public List<ProjectPerformanceItemDto> Items { get; init; } = [];
}

// ── Müşteri Raporu ──

public record CustomerReportItemDto
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = "";
    public string Code { get; init; } = "";
    public int ProjectCount { get; init; }
    public int ActiveProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int TotalAssigned { get; init; }
    public int TotalCompleted { get; init; }
    public decimal CompletionRate { get; init; }
    public decimal TotalBudget { get; init; }
    public decimal TotalDistributed { get; init; }
}

public record CustomerReportSummaryDto
{
    public int TotalCustomers { get; init; }
    public int TotalProjects { get; init; }
    public decimal TotalBudget { get; init; }
    public decimal TotalDistributed { get; init; }
}

public record CustomerReportResultDto
{
    public CustomerReportSummaryDto Summary { get; init; } = new();
    public List<CustomerReportItemDto> Items { get; init; } = [];
}

// ── Ödeme Raporu ──

public record PaymentReportBatchSummaryDto
{
    public int DraftCount { get; init; }
    public int SentCount { get; init; }
    public int CompletedCount { get; init; }
    public decimal DraftAmount { get; init; }
    public decimal SentAmount { get; init; }
    public decimal CompletedAmount { get; init; }
}

public record PaymentReportWithdrawalSummaryDto
{
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal ApprovedAmount { get; init; }
    public decimal RejectedAmount { get; init; }
}

public record PaymentBankDistributionDto
{
    public string BankName { get; init; } = "";
    public int Count { get; init; }
    public decimal TotalAmount { get; init; }
}

public record PaymentReportResultDto
{
    public PaymentReportBatchSummaryDto BatchSummary { get; init; } = new();
    public PaymentReportWithdrawalSummaryDto WithdrawalSummary { get; init; } = new();
    public List<PaymentBankDistributionDto> BankDistribution { get; init; } = [];
}

// ── SMS Raporu ──

public record SmsReportBySourceDto
{
    public string Source { get; init; } = "";
    public int TotalCount { get; init; }
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int SkippedCount { get; init; }
}

public record SmsReportSummaryDto
{
    public int TotalCount { get; init; }
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int SkippedCount { get; init; }
    public decimal SuccessRate { get; init; }
}

public record SmsReportResultDto
{
    public SmsReportSummaryDto Summary { get; init; } = new();
    public List<SmsReportBySourceDto> BySource { get; init; } = [];
}

// ── Bildirim Raporu ──

public record NotificationReportSummaryDto
{
    public int TotalCount { get; init; }
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int ReadCount { get; init; }
    public int UnreadCount { get; init; }
    public decimal ReadRate { get; init; }
    public decimal DeliveryRate { get; init; }
}

public record NotificationReportByTypeDto
{
    public string Type { get; init; } = "";
    public int TotalCount { get; init; }
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int ReadCount { get; init; }
    public int UnreadCount { get; init; }
}

public record NotificationReportResultDto
{
    public NotificationReportSummaryDto Summary { get; init; } = new();
    public List<NotificationReportByTypeDto> ByType { get; init; } = [];
}
