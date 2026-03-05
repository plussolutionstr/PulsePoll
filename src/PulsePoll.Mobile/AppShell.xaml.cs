using PulsePoll.Mobile.Views;

namespace PulsePoll.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider sp)
    {
        InitializeComponent();

        // DI factory — Shell DataTemplate parametresiz constructor arar,
        // bu yüzden sayfaları DI'dan resolve ediyoruz
        homeTab.ContentTemplate = new DataTemplate(() => sp.GetRequiredService<HomePage>());
        surveysTab.ContentTemplate = new DataTemplate(() => sp.GetRequiredService<SurveysPage>());
        historyTab.ContentTemplate = new DataTemplate(() => sp.GetRequiredService<HistoryPage>());
        walletTab.ContentTemplate = new DataTemplate(() => sp.GetRequiredService<WalletPage>());
        profileTab.ContentTemplate = new DataTemplate(() => sp.GetRequiredService<ProfilePage>());

        // Push routes
        Routing.RegisterRoute("surveydetail", typeof(SurveyDetailPage));
        Routing.RegisterRoute("activequestion", typeof(ActiveQuestionPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));
        Routing.RegisterRoute("storyviewer", typeof(StoryViewerPage));
        Routing.RegisterRoute("newsdetail", typeof(NewsDetailPage));
        Routing.RegisterRoute("surveywebview", typeof(SurveyWebViewPage));
        Routing.RegisterRoute("surveyresult", typeof(SurveyResultPage));
        Routing.RegisterRoute("wallet-add-bank", typeof(WalletAddBankAccountPage));
        Routing.RegisterRoute("wallet-withdraw", typeof(WalletWithdrawPage));
        Routing.RegisterRoute("wallet-transactions", typeof(WalletTransactionsPage));
    }
}
