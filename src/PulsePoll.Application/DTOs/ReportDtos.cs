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
