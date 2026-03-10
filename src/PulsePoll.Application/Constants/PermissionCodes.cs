namespace PulsePoll.Application.Constants;

public static class PermissionCodes
{
    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
    }

    public static class Projects
    {
        public const string View = "Projects.View";
        public const string Create = "Projects.Create";
        public const string Edit = "Projects.Edit";
        public const string Delete = "Projects.Delete";
        public const string ManageSubjects = "Projects.ManageSubjects";
        public const string ManageRewards = "Projects.ManageRewards";
        public const string ToggleStatus = "Projects.ToggleStatus";
        public const string StatementView = "Projects.StatementView";
    }

    public static class Subjects
    {
        public const string View = "Subjects.View";
        public const string Create = "Subjects.Create";
        public const string Edit = "Subjects.Edit";
        public const string Approve = "Subjects.Approve";
        public const string Reject = "Subjects.Reject";
        public const string SendSms = "Subjects.SendSms";
        public const string SendPush = "Subjects.SendPush";
        public const string LedgerView = "Subjects.LedgerView";
        public const string LedgerManage = "Subjects.LedgerManage";
        public const string ProjectsView = "Subjects.ProjectsView";
        public const string ReferralsView = "Subjects.ReferralsView";
        public const string ScoringSettings = "Subjects.ScoringSettings";
        public const string RewardUnitSettings = "Subjects.RewardUnitSettings";
    }

    public static class MediaLibrary
    {
        public const string View = "MediaLibrary.View";
        public const string Upload = "MediaLibrary.Upload";
        public const string Delete = "MediaLibrary.Delete";
    }

    public static class Stories
    {
        public const string View = "Stories.View";
        public const string Create = "Stories.Create";
        public const string Edit = "Stories.Edit";
        public const string Delete = "Stories.Delete";
        public const string Reorder = "Stories.Reorder";
    }

    public static class News
    {
        public const string View = "News.View";
        public const string Create = "News.Create";
        public const string Edit = "News.Edit";
        public const string Delete = "News.Delete";
        public const string Reorder = "News.Reorder";
    }

    public static class Payments
    {
        public const string WithdrawalsView = "Payments.WithdrawalsView";
        public const string WithdrawalsApprove = "Payments.WithdrawalsApprove";
        public const string WithdrawalsReject = "Payments.WithdrawalsReject";
        public const string WithdrawalsExport = "Payments.WithdrawalsExport";
        public const string BatchesView = "Payments.BatchesView";
        public const string BatchesCreate = "Payments.BatchesCreate";
        public const string BatchesManage = "Payments.BatchesManage";
        public const string BatchesExport = "Payments.BatchesExport";
        public const string SettingsView = "Payments.SettingsView";
        public const string SettingsEdit = "Payments.SettingsEdit";
    }

    public static class Notifications
    {
        public const string View = "Notifications.View";
    }

    public static class Communications
    {
        public const string View = "Communications.View";
        public const string Edit = "Communications.Edit";
        public const string Sync = "Communications.Sync";
        public const string Run = "Communications.Run";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string ReferralRewardEdit = "Settings.ReferralRewardEdit";
        public const string RegistrationEdit = "Settings.RegistrationEdit";
        public const string AppContentEdit = "Settings.AppContentEdit";
        public const string NotificationDistributionEdit = "Settings.NotificationDistributionEdit";
    }

    public static class AdminUsers
    {
        public const string View = "AdminUsers.View";
        public const string Create = "AdminUsers.Create";
        public const string Edit = "AdminUsers.Edit";
        public const string Activate = "AdminUsers.Activate";
    }

    public static class Affiliates
    {
        public const string View = "Affiliates.View";
        public const string Create = "Affiliates.Create";
        public const string Edit = "Affiliates.Edit";
        public const string Delete = "Affiliates.Delete";
        public const string Pay = "Affiliates.Pay";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Edit = "Roles.Edit";
        public const string Delete = "Roles.Delete";
        public const string ManagePermissions = "Roles.ManagePermissions";
    }
}
