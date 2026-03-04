namespace PulsePoll.Application.Messaging;

// Kuyruk isimleri
public static class Queues
{
    public const string SubjectRegistered = "subject.registered";
    public const string SurveyCompleted = "survey.completed";
    public const string NotificationSend = "notification.send";
    public const string NotificationSendFault = "notification.send.fault";
    public const string WithdrawalRequested = "withdrawal.requested";
    public const string WalletCredit              = "wallet.credit";
    public const string SubjectAssignmentRequested = "subject.assignment.requested";
    public const string SmsSend = "sms.send";
    public const string SmsSendFault = "sms.send.fault";
    public const string CommunicationAutomationScheduleChanged = "communication.automation.schedule.changed";
}

// Mesaj modelleri
public record SubjectRegisteredMessage(
    string Email,
    string FirstName,
    string LastName,
    string PasswordHash,
    string PhoneNumber,
    int Gender,
    int MaritalStatus,
    int GsmOperator,
    DateOnly BirthDate,
    int CityId,
    int DistrictId,
    bool IsRetired,
    int ProfessionId,
    int EducationLevelId,
    bool IsHeadOfFamily,
    bool IsHeadOfFamilyRetired,
    int? HeadOfFamilyProfessionId,
    int? HeadOfFamilyEducationLevelId,
    int BankId,
    string IBAN,
    string IBANFullName,
    int SocioeconomicStatusId,
    int LSMSocioeconomicStatusId,
    string? ReferenceCode,
    int? SpecialCodeId,
    bool KVKKApproval,
    string? KVKKDetail,
    DateTime RegisteredAt);

public record SurveyCompletedMessage(
    int ProjectId,
    int SubjectId,
    decimal RewardAmount,
    string WebhookPayload,
    DateTime CompletedAt);

public record NotificationSendMessage(
    int NotificationId,
    int SubjectId,
    string FcmToken,
    string Title,
    string Body,
    string? Type);

public record WithdrawalRequestedMessage(
    int SubjectId,
    int BankAccountId,
    decimal Amount,
    decimal AmountTry,
    string UnitCode,
    string UnitLabel,
    decimal UnitTryMultiplier,
    int WalletTransactionId,
    DateTime RequestedAt);

public record WalletCreditMessage(
    int SubjectId,
    decimal Amount,
    string ReferenceId,
    string Description);

public record SubjectAssignmentRequestedMessage(
    int JobId,
    int ProjectId,
    int[] SubjectIds,
    int AdminId);

public record SmsSendMessage(
    string PhoneNumber,
    string Message,
    int? SubjectId,
    int? SentByAdminId);

public record CommunicationAutomationScheduleChangedMessage(
    string DailyRunTime,
    string TimeZoneId,
    int UpdatedByAdminId,
    DateTime UpdatedAtUtc);
