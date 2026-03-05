using Microsoft.EntityFrameworkCore;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TaxOffice> TaxOffices => Set<TaxOffice>();

    // Lookup tables
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<SocioeconomicStatus> SocioeconomicStatuses => Set<SocioeconomicStatus>();
    public DbSet<LSMSocioeconomicStatus> LSMSocioeconomicStatuses => Set<LSMSocioeconomicStatus>();
    public DbSet<SpecialCode> SpecialCodes => Set<SpecialCode>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<SurveyResultScript> SurveyResultScripts => Set<SurveyResultScript>();
    public DbSet<SurveyResultPattern> SurveyResultPatterns => Set<SurveyResultPattern>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryView> StoryViews => Set<StoryView>();
    public DbSet<News> News => Set<News>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
    public DbSet<SubjectAssignmentJob> SubjectAssignmentJobs => Set<SubjectAssignmentJob>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<ReferralRewardConfig> ReferralRewardConfigs => Set<ReferralRewardConfig>();
    public DbSet<AppContentConfig> AppContentConfigs => Set<AppContentConfig>();
    public DbSet<SpecialDay> SpecialDays => Set<SpecialDay>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<MessageCampaign> MessageCampaigns => Set<MessageCampaign>();
    public DbSet<MessageDispatchLog> MessageDispatchLogs => Set<MessageDispatchLog>();
    public DbSet<CommunicationAutomationConfig> CommunicationAutomationConfigs => Set<CommunicationAutomationConfig>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
    public DbSet<PaymentBatch> PaymentBatches => Set<PaymentBatch>();
    public DbSet<PaymentBatchItem> PaymentBatchItems => Set<PaymentBatchItem>();
    public DbSet<PaymentSetting> PaymentSettings => Set<PaymentSetting>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<SubjectScoreSnapshot> SubjectScoreSnapshots => Set<SubjectScoreSnapshot>();
    public DbSet<SubjectAppActivity> SubjectAppActivities => Set<SubjectAppActivity>();
    public DbSet<SubjectScoreConfig> SubjectScoreConfigs => Set<SubjectScoreConfig>();
    public DbSet<RewardUnitConfig> RewardUnitConfigs => Set<RewardUnitConfig>();

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<AdminUserRole> AdminUserRoles => Set<AdminUserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
