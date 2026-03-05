using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PulsePoll.Mobile.Models;

public record StoryModel(
    int Id,
    string Title,
    string ImageUrl,
    string StoryImageUrl = "",
    string? LinkUrl = null,
    string Description = "",
    string BrandColor = "#7C5CFC",
    bool IsSeen = false)
{
    public string DisplayStoryImageUrl => string.IsNullOrWhiteSpace(StoryImageUrl) ? ImageUrl : StoryImageUrl;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public Brush RingStroke => new SolidColorBrush(Color.FromArgb(IsSeen ? "#B8BCC5" : BrandColor));
}

public record NewsModel(
    int Id,
    string Tag,
    string Title,
    string Subtitle,
    string GradientStart = "#2D1B6E",
    string GradientEnd = "#A78BFA",
    string ImageUrl = "",
    string? LinkUrl = null)
{
    public bool HasLink => !string.IsNullOrWhiteSpace(LinkUrl);
}

public record SurveyModel(
    int Id,
    string BrandName,
    string BrandLogoUrl,
    string Title,
    string Description,
    string Category,
    decimal Reward,
    int DurationMinutes,
    int QuestionCount,
    List<SurveyCriteria> Criteria,
    string BannerGradientStart = "#EDE8FF",
    string BannerGradientEnd = "#DDD6FE",
    string BrandColor = "#7C5CFC")
{
    public bool HasCategory => !string.IsNullOrWhiteSpace(Category);
}

public record SurveyCriteria(string Label, bool IsMet);

public record QuestionModel(int Number, string Text, List<OptionModel> Options);

public record OptionModel(int Id, string Text);

public record ProfileModel(
    string FullName,
    string Email,
    string AvatarUrl,
    string Tier,
    int Points,
    int CompletedCount,
    int DisqualifiedCount,
    int SuccessRate,
    List<DemographicField> Demographics,
    List<string> Interests);

public record DemographicField(string Label, string Value);

public record HistoryItemModel(
    int Id,
    string SurveyTitle,
    string BrandName,
    string Status, // Tamamlandı, Elendi, Devam Ediyor
    decimal? Reward,
    DateTime Date,
    int QuestionCount);

public record HistoryGroup(string Month, List<HistoryItemModel> Items);

public record WalletModel(
    decimal WithdrawableBalance,
    int Points,
    decimal TotalEarned,
    List<BankAccountModel> BankAccounts,
    List<TransactionModel> RecentTransactions);

public record BankAccountModel(int Id, string BankName, string MaskedIban);

public record TransactionModel(
    int Id,
    string Title,
    string Description,
    decimal Amount,
    DateTime Date,
    bool IsIncome);

public record NotificationModel(
    int Id,
    string Type, // survey, earning, rank, disqualified, system
    string Title,
    string Message,
    DateTime Date,
    bool IsRead);
