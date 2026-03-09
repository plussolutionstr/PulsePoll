using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.ApiModels;

public static class ApiDtoMapper
{
    public static StoryModel ToModel(this StoryApiDto dto)
        => new(
            dto.Id,
            dto.Title,
            dto.ImageUrl,
            dto.StoryImageUrl ?? dto.ImageUrl,
            dto.LinkUrl,
            dto.Description ?? string.Empty,
            IsSeen: dto.IsSeen);

    public static NewsModel ToModel(this NewsApiDto dto)
        => new(dto.Id, "", dto.Title, dto.Summary, ImageUrl: dto.ImageUrl, LinkUrl: dto.LinkUrl);

    public static HistoryItemModel ToModel(this ProjectHistoryApiDto dto)
        => new(
            dto.ProjectId,
            dto.ProjectName,
            ToHistoryStatus(dto.Status),
            dto.EarnedAmount > 0m ? dto.EarnedAmount : null,
            dto.CompletedAt ?? dto.AssignedAt,
            0,
            dto.RewardUnitLabel,
            dto.DurationMinutes,
            dto.RewardAmount,
            dto.ConsolationRewardAmount,
            dto.RewardStatus.ToString());

    public static SurveyModel ToModel(this ProjectApiDto dto)
        => new(
            dto.Id,
            dto.CustomerShortName,
            dto.CoverImageUrl ?? "",
            dto.Name,
            dto.Description ?? "",
            dto.Category ?? "",
            dto.Reward,
            dto.EstimatedMinutes,
            0,
            [],
            ConsolationReward: dto.ConsolationReward,
            RewardUnitLabel: dto.RewardUnitLabel,
            SurveyUrl: dto.SurveyUrl,
            SubjectParameterName: dto.SubjectParameterName,
            StartMessage: dto.StartMessage,
            CompletedMessage: dto.CompletedMessage,
            DisqualifyMessage: dto.DisqualifyMessage,
            QuotaFullMessage: dto.QuotaFullMessage,
            ScreenOutMessage: dto.ScreenOutMessage,
            SupportsSurveyHelper: dto.SupportsSurveyHelper,
            AssignmentStatus: dto.AssignmentStatus?.ToString());

    private static string ToHistoryStatus(AssignmentStatus status) => status switch
    {
        AssignmentStatus.Completed => "Tamamlandı",
        AssignmentStatus.Partial => "Devam Ediyor",
        AssignmentStatus.NotStarted => "Devam Ediyor",
        AssignmentStatus.Disqualify => "Diskalifiye",
        AssignmentStatus.QuotaFull => "Kota Dolu",
        AssignmentStatus.ScreenOut => "Elenmiş",
        _ => "Bilinmiyor"
    };

    public static BankAccountModel ToModel(this BankAccountApiDto dto)
        => new BankAccountModel(
            dto.Id,
            dto.BankName,
            $"TR** **** **** **** **** {dto.IbanLast4}",
            dto.IbanLast4,
            dto.IsDefault)
        {
            ThumbnailImageUrl = dto.ThumbnailImageUrl,
            LogoImageUrl = dto.LogoImageUrl,
            CanDelete = dto.CanDelete,
            DeleteCooldownEndsAt = dto.DeleteCooldownEndsAt,
            CanWithdraw = dto.CanWithdraw,
            WithdrawalCooldownEndsAt = dto.WithdrawalCooldownEndsAt
        };

    public static BankOptionModel ToModel(this BankOptionApiDto dto)
        => new(dto.Id, dto.Name, dto.Code, dto.BankCode, dto.ThumbnailImageUrl, dto.LogoImageUrl);

    public static TransactionModel ToModel(this WalletTransactionApiDto dto)
    {
        var isIncome = dto.Type == WalletTransactionType.Credit;
        var title = ResolveTransactionTitle(dto);
        var description = string.IsNullOrWhiteSpace(dto.Description)
            ? (isIncome ? "Kazanç hareketi" : "Çekim hareketi")
            : dto.Description!;

        return new TransactionModel(
            dto.Id,
            title,
            description,
            isIncome ? dto.Amount : -dto.Amount,
            dto.CreatedAt,
            isIncome,
            dto.UnitLabel);
    }

    private static string ResolveTransactionTitle(WalletTransactionApiDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Description) &&
            dto.Description.StartsWith("Anket ödülü:", StringComparison.OrdinalIgnoreCase))
        {
            return "Anket Ödülü";
        }

        return dto.Type switch
        {
            WalletTransactionType.Credit => "Bakiye Girişi",
            WalletTransactionType.Withdrawal => "Para Çekme",
            _ => "Cüzdan Hareketi"
        };
    }
}
