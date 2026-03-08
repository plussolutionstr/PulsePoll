using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PulsePoll.Mobile.Helpers;

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
    string BrandColor = "#7C5CFC",
    decimal ConsolationReward = 0m,
    string RewardUnitLabel = "TL",
    string SurveyUrl = "",
    string SubjectParameterName = "uid",
    string StartMessage = "",
    string CompletedMessage = "",
    string DisqualifyMessage = "",
    string QuotaFullMessage = "",
    string ScreenOutMessage = "",
    string? AssignmentStatus = null)
{
    public bool HasCategory => !string.IsNullOrWhiteSpace(Category);
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public string RewardDisplay => $"{Reward:0.##} {RewardUnitLabel}";
    public string ConsolationRewardDisplay => $"{ConsolationReward:0.##} {RewardUnitLabel}";
    public string StartMessageDisplay => string.IsNullOrWhiteSpace(StartMessage)
        ? "Ankete başlamadan önce soruları dikkatle okuyunuz."
        : StartMessage;
}

public record SurveyCriteria(string Label, bool IsMet);

public record QuestionModel(int Number, string Text, List<OptionModel> Options);

public record OptionModel(int Id, string Text);

public record DemographicField(string Label, string Value);

public record HistoryItemModel(
    int Id,
    string SurveyTitle,
    string Status, // Tamamlandı, Elendi, Devam Ediyor
    decimal? Reward,
    DateTime Date,
    int QuestionCount,
    string RewardUnitLabel = "Poll",
    int DurationMinutes = 0,
    decimal RewardAmount = 0m,
    decimal ConsolationRewardAmount = 0m,
    string RewardStatus = "None")
{
    public string RewardAmountDisplay => $"{RewardAmount:0.##} {RewardUnitLabel}";
    public string ConsolationAmountDisplay => $"{ConsolationRewardAmount:0.##} {RewardUnitLabel}";
}

public record HistoryGroup(string Month, List<HistoryItemModel> Items);

public record WalletModel(
    decimal WithdrawableBalance,
    string PointsLabel,
    decimal TotalEarned,
    string RewardUnitLabel,
    List<BankAccountModel> BankAccounts,
    List<TransactionModel> RecentTransactions);

public record BankAccountModel(int Id, string BankName, string MaskedIban, string IbanLast4, bool IsDefault = false)
{
    public string DisplayTitle => $"{BankName} {IbanLast4}";
    public string? ThumbnailImageUrl { get; init; }
    public string? LogoImageUrl { get; init; }
    public string DisplayImageUrl => !string.IsNullOrWhiteSpace(ThumbnailImageUrl)
        ? ThumbnailImageUrl!
        : (LogoImageUrl ?? string.Empty);
    public bool CanDelete { get; init; } = true;
    public DateTime? DeleteCooldownEndsAt { get; init; }
    public bool CanWithdraw { get; init; } = true;
    public DateTime? WithdrawalCooldownEndsAt { get; init; }
    public string? CooldownMessage
    {
        get
        {
            if (!CanDelete && DeleteCooldownEndsAt.HasValue)
                return $"{DeleteCooldownEndsAt.Value:dd.MM.yyyy} tarihine kadar silinemez";
            if (!CanWithdraw && WithdrawalCooldownEndsAt.HasValue)
                return $"{WithdrawalCooldownEndsAt.Value:dd.MM.yyyy} tarihine kadar çekim yapılamaz";
            return null;
        }
    }
}
public record BankOptionModel(
    int Id,
    string Name,
    string? Code = null,
    string? ThumbnailImageUrl = null,
    string? LogoImageUrl = null)
{
    public string DisplayImageUrl => !string.IsNullOrWhiteSpace(ThumbnailImageUrl)
        ? ThumbnailImageUrl!
        : (LogoImageUrl ?? string.Empty);
}

public record TransactionModel(
    int Id,
    string Title,
    string Description,
    decimal Amount,
    DateTime Date,
    bool IsIncome,
    string UnitLabel = "Poll")
{
    public string AmountDisplay => $"{Amount:+0.##;-0.##;0} {UnitLabel}";
    public string IconGlyph => IsIncome ? "↓" : "↑";
    public Color IconColor => IsIncome
        ? ThemeHelper.Resolve("Success", "SuccessDark", Colors.Green)
        : ThemeHelper.Resolve("PrimaryPurple", "PrimaryPurpleDark", Colors.Purple);
    public Color IconBgColor => IsIncome
        ? ThemeHelper.Resolve("SuccessLight", "SuccessLightDark", Colors.LightGreen)
        : ThemeHelper.Resolve("PrimaryLight", "PrimaryLightDark", Colors.Lavender);
}

public record NotificationModel(
    int Id,
    string Type, // survey, earning, rank, disqualified, system
    string Title,
    string Message,
    DateTime Date,
    bool IsRead);
