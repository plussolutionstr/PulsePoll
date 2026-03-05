using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.Constants;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class PaymentBatchService(
    IWithdrawalRequestRepository withdrawalRepo,
    IPaymentBatchRepository batchRepo,
    IPaymentSettingRepository settingRepo,
    IWalletRepository walletRepo,
    ILogger<PaymentBatchService> logger) : IPaymentBatchService
{
    // ── Withdrawal management ──────────────────────────────────────

    public async Task<(List<WithdrawalRequestAdminDto> Items, int Total)> GetPendingWithdrawalsAsync(int skip, int take)
    {
        var (items, total) = await withdrawalRepo.GetPagedAsync(ApprovalStatus.Pending, skip, take);
        return (items.Select(ToAdminDto).ToList(), total);
    }

    public async Task<(List<WithdrawalRequestAdminDto> Items, int Total)> GetApprovedWithdrawalsAsync(int skip, int take)
    {
        var (items, total) = await withdrawalRepo.GetApprovedWithoutBatchPagedAsync(skip, take);
        return (items.Select(ToAdminDto).ToList(), total);
    }

    public async Task ApproveWithdrawalAsync(int requestId, int adminId)
    {
        var request = await withdrawalRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("Para çekimi talebi");

        if (request.Status != ApprovalStatus.Pending)
            throw new BusinessException("ALREADY_PROCESSED", "Bu talep zaten işlenmiş.");

        request.Status      = ApprovalStatus.Approved;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = adminId;
        request.SetUpdated(adminId);
        await withdrawalRepo.UpdateAsync(request);

        logger.LogInformation("Para çekimi onaylandı: RequestId={RequestId} AdminId={AdminId}", requestId, adminId);
    }

    public async Task RejectWithdrawalAsync(int requestId, string reason, int adminId)
    {
        var request = await withdrawalRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("Para çekimi talebi");

        if (request.Status != ApprovalStatus.Pending)
            throw new BusinessException("ALREADY_PROCESSED", "Bu talep zaten işlenmiş.");

        request.Status          = ApprovalStatus.Rejected;
        request.RejectionReason = reason;
        request.ProcessedAt     = DateTime.UtcNow;
        request.ProcessedBy     = adminId;
        request.SetUpdated(adminId);

        // İadeyi cüzdana ekle
        var wallet = await walletRepo.GetBySubjectIdAsync(request.SubjectId)
            ?? throw new NotFoundException("Cüzdan");

        wallet.Balance += request.Amount;
        wallet.SetUpdated(0);
        await walletRepo.UpdateAsync(wallet);

        var refundTx = new WalletTransaction
        {
            WalletId    = wallet.Id,
            Amount      = request.Amount,
            Type        = WalletTransactionType.Credit,
            ReferenceId = $"refund:{request.WalletTransactionId}",
            Description = "Para çekimi iadesi"
        };
        refundTx.SetCreated(0);
        await walletRepo.AddTransactionAsync(refundTx);

        await withdrawalRepo.UpdateAsync(request);

        logger.LogInformation("Para çekimi reddedildi: RequestId={RequestId} AdminId={AdminId} Reason={Reason}",
            requestId, adminId, reason);
    }

    // ── Batch management ──────────────────────────────────────────

    public async Task<(List<PaymentBatchDto> Items, int Total)> GetBatchesAsync(PaymentBatchStatus? status, int skip, int take)
    {
        var (items, total) = await batchRepo.GetPagedAsync(status, skip, take);
        return (items.Select(ToBatchDto).ToList(), total);
    }

    public async Task<PaymentBatchDetailDto> GetBatchDetailAsync(int batchId)
    {
        var batch = await batchRepo.GetDetailAsync(batchId)
            ?? throw new NotFoundException("Ödeme paketi");

        return ToBatchDetailDto(batch);
    }

    public async Task<PaymentBatchDto> CreateBatchAsync(CreatePaymentBatchDto dto, int adminId)
    {
        if (dto.WithdrawalRequestIds.Count == 0)
            throw new BusinessException("NO_ITEMS", "En az bir talep seçilmelidir.");

        var approved = await withdrawalRepo.GetApprovedWithoutBatchAsync();
        var selectedMap = approved.ToDictionary(r => r.Id);

        var notFound = dto.WithdrawalRequestIds.Where(id => !selectedMap.ContainsKey(id)).ToList();
        if (notFound.Count > 0)
            throw new BusinessException("INVALID_REQUESTS",
                $"Şu talepler onaylı değil veya zaten bir pakete eklenmiş: {string.Join(", ", notFound)}");

        var selectedRequests = dto.WithdrawalRequestIds.Select(id => selectedMap[id]).ToList();

        // Batch numarası: PB-YYYYMMDD-NNN
        var datePrefix  = DateTime.UtcNow.ToString("yyyyMMdd");
        var seq         = await batchRepo.GetNextSequenceAsync(datePrefix);
        var batchNumber = $"PB-{datePrefix}-{seq:D3}";

        var batch = new PaymentBatch
        {
            BatchNumber = batchNumber,
            Status      = PaymentBatchStatus.Draft,
            TotalAmount = selectedRequests.Sum(r => r.AmountTry),
            ItemCount   = selectedRequests.Count,
            Note        = dto.Note,
        };
        batch.SetCreated(adminId);

        batch.Items = selectedRequests.Select(r =>
        {
            var item = new PaymentBatchItem
            {
                WithdrawalRequestId = r.Id,
                Status              = PaymentStatus.Pending
            };
            item.SetCreated(adminId);
            return item;
        }).ToList();

        await batchRepo.AddAsync(batch);

        logger.LogInformation("Ödeme paketi oluşturuldu: {BatchNumber} ItemCount={ItemCount} AdminId={AdminId}",
            batchNumber, batch.ItemCount, adminId);

        return ToBatchDto(batch);
    }

    public async Task MarkBatchSentAsync(int batchId, int adminId)
    {
        var batch = await batchRepo.GetByIdAsync(batchId)
            ?? throw new NotFoundException("Ödeme paketi");

        if (batch.Status != PaymentBatchStatus.Draft)
            throw new BusinessException("INVALID_STATUS", "Yalnızca Taslak paketler gönderilebilir.");

        batch.Status  = PaymentBatchStatus.Sent;
        batch.SentAt  = DateTime.UtcNow;
        batch.SetUpdated(adminId);
        await batchRepo.UpdateAsync(batch);

        logger.LogInformation("Ödeme paketi gönderildi: {BatchNumber} AdminId={AdminId}", batch.BatchNumber, adminId);
    }

    public async Task UpdateItemStatusAsync(int batchId, int itemId, UpdatePaymentItemStatusDto dto, int adminId)
    {
        var batch = await batchRepo.GetDetailAsync(batchId)
            ?? throw new NotFoundException("Ödeme paketi");

        if (batch.Status == PaymentBatchStatus.Completed)
            throw new BusinessException("BATCH_COMPLETED", "Tamamlanmış pakette değişiklik yapılamaz.");

        var item = batch.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("Ödeme kalemi");

        item.Status        = dto.Status;
        item.FailureReason = dto.FailureReason;
        item.ProcessedAt   = DateTime.UtcNow;
        item.SetUpdated(adminId);

        batch.SetUpdated(adminId);
        await batchRepo.UpdateAsync(batch);
    }

    public async Task FinalizeBatchAsync(int batchId, int adminId)
    {
        var batch = await batchRepo.GetDetailAsync(batchId)
            ?? throw new NotFoundException("Ödeme paketi");

        if (batch.Status != PaymentBatchStatus.Sent)
            throw new BusinessException("INVALID_STATUS", "Yalnızca Gönderildi statüsündeki paketler tamamlanabilir.");

        var pendingItems = batch.Items.Where(i => i.Status == PaymentStatus.Pending).ToList();
        if (pendingItems.Count > 0)
            throw new BusinessException("PENDING_ITEMS",
                $"{pendingItems.Count} kalem hâlâ beklemede. Önce tüm kalemlerin durumunu güncelleyin.");

        batch.Status      = PaymentBatchStatus.Completed;
        batch.CompletedAt = DateTime.UtcNow;
        batch.SetUpdated(adminId);
        await batchRepo.UpdateAsync(batch);

        logger.LogInformation("Ödeme paketi tamamlandı: {BatchNumber} AdminId={AdminId}", batch.BatchNumber, adminId);
    }

    public async Task<PaymentExportFileDto> GenerateBankTransferFileAsync(int batchId)
    {
        var batch = await GetBatchDetailAsync(batchId);
        var settings = await GetSettingsAsync();
        var settingsMap = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

        var fileTemplate = GetSettingValue(
            settingsMap,
            PaymentSettingKeys.BankTransferFileTemplate,
            PaymentFileTemplateDefaults.BankTransferTemplate);
        var fileNameTemplate = GetSettingValue(
            settingsMap,
            PaymentSettingKeys.BankTransferFileNameTemplate,
            PaymentFileTemplateDefaults.BankTransferFileNameTemplate);

        var now = DateTime.Now;
        var (headerTemplate, detailTemplate, trailerTemplate) = ParseBankTransferTemplate(fileTemplate);
        var detailLines = batch.Items.Select(i => BuildDetailLine(i, detailTemplate, now)).ToList();

        var header = ApplyTokens(headerTemplate, BuildGlobalTokenMap(batch, now, detailLines.Count));
        var trailer = ApplyTokens(trailerTemplate, BuildGlobalTokenMap(batch, now, detailLines.Count));
        var lines = new List<string>(detailLines.Count + 2) { header };
        lines.AddRange(detailLines);
        lines.Add(trailer);

        var content = string.Join("\r\n", lines) + "\r\n";
        var fileName = BuildFileName(fileNameTemplate, batch, now, detailLines.Count);
        var bytes = GetBankFileEncoding().GetBytes(content);

        logger.LogInformation(
            "Ödeme paketi banka dosyası üretildi: BatchId={BatchId} BatchNumber={BatchNumber} ItemCount={ItemCount}",
            batch.Id,
            batch.BatchNumber,
            detailLines.Count);

        return new PaymentExportFileDto(fileName, bytes, "text/plain;charset=iso-8859-9;");
    }

    // ── Settings ──────────────────────────────────────────────────

    public async Task<List<PaymentSettingDto>> GetSettingsAsync()
    {
        var settings = await settingRepo.GetAllAsync();
        return settings.Select(s => new PaymentSettingDto(s.Id, s.Key, s.Value, s.Description)).ToList();
    }

    public async Task UpsertSettingAsync(string key, string value, int adminId, string? description = null)
    {
        if (string.Equals(key, PaymentSettingKeys.WithdrawalMinAmountTry, StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParseDecimal(value, out var minTry) || minTry < 0m)
            {
                throw new BusinessException(
                    "INVALID_WITHDRAWAL_MIN_THRESHOLD",
                    "Minimum çekim eşiği değeri 0 veya daha büyük bir sayı olmalıdır.");
            }

            value = minTry.ToString("0.##", CultureInfo.InvariantCulture);
        }
        else if (string.Equals(key, PaymentSettingKeys.BankTransferFileTemplate, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("INVALID_BANK_FILE_TEMPLATE", "Banka dosya şablonu boş olamaz.");
        }
        else if (string.Equals(key, PaymentSettingKeys.BankTransferFileNameTemplate, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("INVALID_BANK_FILE_NAME_TEMPLATE", "Banka dosya adı şablonu boş olamaz.");
        }

        await settingRepo.UpsertAsync(key, value, adminId, description);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static WithdrawalRequestAdminDto ToAdminDto(WithdrawalRequest r) => new(
        r.Id,
        r.SubjectId,
        r.Subject?.FullName ?? "-",
        r.BankAccountId,
        r.BankAccount?.BankName ?? "-",
        r.BankAccount?.IbanLast4 ?? "-",
        r.BankAccount?.IbanEncrypted ?? r.BankAccount?.IbanLast4 ?? "-",
        r.Amount,
        r.AmountTry,
        r.UnitCode,
        r.UnitLabel,
        r.UnitTryMultiplier,
        r.Status.ToString(),
        r.CreatedAt,
        r.RejectionReason,
        r.ProcessedAt);

    private static PaymentBatchDto ToBatchDto(PaymentBatch b) => new(
        b.Id,
        b.BatchNumber,
        b.Status,
        b.Status switch
        {
            PaymentBatchStatus.Draft      => "Taslak",
            PaymentBatchStatus.Sent       => "Gönderildi",
            PaymentBatchStatus.Completed  => "Tamamlandı",
            _                             => b.Status.ToString()
        },
        b.TotalAmount,
        b.ItemCount,
        b.Note,
        b.CreatedAt,
        b.SentAt,
        b.CompletedAt);

    private static PaymentBatchDetailDto ToBatchDetailDto(PaymentBatch b) => new(
        b.Id,
        b.BatchNumber,
        b.Status,
        b.Status switch
        {
            PaymentBatchStatus.Draft      => "Taslak",
            PaymentBatchStatus.Sent       => "Gönderildi",
            PaymentBatchStatus.Completed  => "Tamamlandı",
            _                             => b.Status.ToString()
        },
        b.TotalAmount,
        b.ItemCount,
        b.Note,
        b.CreatedAt,
        b.SentAt,
        b.CompletedAt,
        b.Items.Select(i => new PaymentBatchItemDto(
            i.Id,
            i.WithdrawalRequestId,
            i.WithdrawalRequest?.SubjectId ?? 0,
            i.WithdrawalRequest?.Subject?.FullName ?? "-",
            i.WithdrawalRequest?.Subject?.PhoneNumber ?? "-",
            i.WithdrawalRequest?.BankAccount?.BankName ?? "-",
            i.WithdrawalRequest?.BankAccount?.IbanLast4 ?? "-",
            i.WithdrawalRequest?.BankAccount?.IbanEncrypted,
            i.WithdrawalRequest?.Amount ?? 0,
            i.WithdrawalRequest?.AmountTry ?? 0,
            i.WithdrawalRequest?.UnitLabel ?? "TL",
            i.Status,
            i.Status switch
            {
                PaymentStatus.Pending => "Beklemede",
                PaymentStatus.Paid    => "Ödendi",
                PaymentStatus.Failed  => "Başarısız",
                _                    => i.Status.ToString()
            },
            i.FailureReason,
            i.ProcessedAt)).ToList());

    private static bool TryParseDecimal(string rawValue, out decimal value)
    {
        var normalized = rawValue.Trim();
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return true;

        if (decimal.TryParse(normalized, NumberStyles.Number, new CultureInfo("tr-TR"), out value))
            return true;

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }

    private static Encoding GetBankFileEncoding()
    {
        try
        {
            return Encoding.GetEncoding("iso-8859-9");
        }
        catch
        {
            return Encoding.UTF8;
        }
    }

    private static string GetSettingValue(
        IReadOnlyDictionary<string, string> settings,
        string key,
        string fallback)
        => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private static (string HeaderTemplate, string DetailTemplate, string TrailerTemplate) ParseBankTransferTemplate(string template)
    {
        var lines = template
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length >= 3)
            return (lines[0], lines[1], lines[2]);

        var fallbackLines = PaymentFileTemplateDefaults.BankTransferTemplate
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return (fallbackLines[0], fallbackLines[1], fallbackLines[2]);
    }

    private static string BuildDetailLine(PaymentBatchItemDto item, string detailTemplate, DateTime now)
    {
        var reference = item.WithdrawalRequestId.ToString(CultureInfo.InvariantCulture);
        var fullName = (item.SubjectFullName ?? string.Empty).Trim();
        var maskedName = MaskName(fullName);
        var description = $"DEEMY Hediyesi {reference} {maskedName}".Trim();
        var iban = (item.IbanEncrypted ?? item.IbanLast4 ?? string.Empty).Trim().ToUpperInvariant();

        var detailTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{DATE_DDMMYYYY}"] = now.ToString("ddMMyyyy", CultureInfo.InvariantCulture),
            ["{DATE_YYYYMMDD}"] = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            ["{TIME_HHMMSS}"] = now.ToString("HHmmss", CultureInfo.InvariantCulture),
            ["{IBAN}"] = iban,
            ["{REFERENCE}"] = reference,
            ["{REFERENCE_7}"] = Fit(reference, 7, leftPad: false),
            ["{NAME}"] = fullName,
            ["{NAME_130}"] = Fit(fullName, 130, leftPad: false),
            ["{DESCRIPTION}"] = description,
            ["{DESCRIPTION_56}"] = Fit(description, 56, leftPad: false),
            ["{EMAIL}"] = PaymentFileTemplateDefaults.DefaultContactEmail,
            ["{EMAIL_40}"] = Fit(PaymentFileTemplateDefaults.DefaultContactEmail, 40, leftPad: false),
            ["{AMOUNT_18}"] = FormatAmount(item.AmountTry),
            ["{CURRENCY}"] = PaymentFileTemplateDefaults.DefaultCurrencyCode
        };

        return ApplyTokens(detailTemplate, detailTokens);
    }

    private static IReadOnlyDictionary<string, string> BuildGlobalTokenMap(
        PaymentBatchDetailDto batch,
        DateTime now,
        int count)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{DATE_DDMMYYYY}"] = now.ToString("ddMMyyyy", CultureInfo.InvariantCulture),
            ["{DATE_YYYYMMDD}"] = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            ["{TIME_HHMMSS}"] = now.ToString("HHmmss", CultureInfo.InvariantCulture),
            ["{BATCH_NUMBER}"] = batch.BatchNumber,
            ["{COUNT}"] = count.ToString(CultureInfo.InvariantCulture),
            ["{COUNT_5}"] = count.ToString("D5", CultureInfo.InvariantCulture)
        };

    private static string BuildFileName(string template, PaymentBatchDetailDto batch, DateTime now, int count)
    {
        var fileName = ApplyTokens(template, BuildGlobalTokenMap(batch, now, count)).Trim();
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = ApplyTokens(PaymentFileTemplateDefaults.BankTransferFileNameTemplate, BuildGlobalTokenMap(batch, now, count));

        if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            fileName += ".txt";

        foreach (var ch in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(ch, '_');

        return fileName;
    }

    private static string ApplyTokens(string template, IReadOnlyDictionary<string, string> tokens)
    {
        var output = template;
        foreach (var pair in tokens.OrderByDescending(k => k.Key.Length))
            output = output.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);
        return output;
    }

    private static string Fit(string value, int length, bool leftPad)
    {
        var normalized = value ?? string.Empty;
        if (normalized.Length > length)
            return normalized[..length];

        return leftPad
            ? normalized.PadLeft(length, ' ')
            : normalized.PadRight(length, ' ');
    }

    private static string FormatAmount(decimal value)
    {
        var normalized = value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
        if (normalized.Length > 18)
            throw new InvalidOperationException($"Tutar alanı 18 karakter sınırını aşıyor: {value}");

        return normalized.PadLeft(18, '0');
    }

    private static string MaskName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Length <= 1 ? p : p[0] + new string('*', p.Length - 1));

        return string.Join(' ', parts);
    }
}
