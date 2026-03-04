namespace PulsePoll.Application.Constants;

public static class PaymentFileTemplateDefaults
{
    public const string BankTransferTemplate =
        "BDEEMYARASTIRMAV{DATE_DDMMYYYY}\n" +
        "DTR290013400002002220200002 00134                  {IBAN} {REFERENCE_7}{NAME_130}{DESCRIPTION_56}{EMAIL_40}{AMOUNT_18}TRY  {DATE_DDMMYYYY}000000\n" +
        "T{COUNT_5}";

    public const string BankTransferFileNameTemplate = "OD{DATE_YYYYMMDD}{BATCH_NUMBER}.txt";

    public const string DefaultContactEmail = "muhasebe@remindarastirma.com";
    public const string DefaultCurrencyCode = "TRY";
}
