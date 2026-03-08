namespace PulsePoll.Application.Constants;

public static class PaymentSettingKeys
{
    // Minimum withdrawal threshold in TRY. Example: "100" means requests below 100 TRY are not allowed.
    public const string WithdrawalMinAmountTry = "withdrawal.min_amount_try";

    // Bank transfer file template has three lines:
    // 1) Header template
    // 2) Detail template (repeated for each payment row)
    // 3) Trailer template
    public const string BankTransferFileTemplate = "withdrawal.bank_transfer_file_template";

    // File name template used for export file name generation.
    public const string BankTransferFileNameTemplate = "withdrawal.bank_transfer_file_name_template";

    // IBAN security cooldowns (days)
    public const string IbanDeleteCooldownDays = "iban.delete_cooldown_days";
    public const string IbanWithdrawalCooldownDays = "iban.withdrawal_cooldown_days";
}
