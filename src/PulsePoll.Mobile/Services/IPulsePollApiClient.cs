using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface IPulsePollApiClient
{
    // Auth
    Task<bool> LoginAsync(string phoneNumber, string password, CancellationToken ct = default);
    Task SendOtpAsync(string phoneNumber, CancellationToken ct = default);
    Task<string> VerifyOtpAsync(string phoneNumber, string otp, CancellationToken ct = default);
    Task RegisterAsync(object dto, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> TryRefreshSessionAsync(CancellationToken ct = default);
    Task SendPasswordResetOtpAsync(string phoneNumber, CancellationToken ct = default);
    Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword, CancellationToken ct = default);

    // Stories & News
    Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default);
    Task MarkStorySeenAsync(int storyId, CancellationToken ct = default);
    Task<List<NewsModel>> GetNewsAsync(CancellationToken ct = default);

    // Projects
    Task<List<SurveyModel>> GetProjectsAsync(CancellationToken ct = default);
    Task<SurveyModel?> GetProjectByIdAsync(int projectId, CancellationToken ct = default);
    Task<List<HistoryItemModel>> GetHistoryAsync(CancellationToken ct = default);
    Task<string> StartProjectAsync(int projectId, CancellationToken ct = default);
    // Wallet
    Task<WalletApiDto?> GetWalletAsync(CancellationToken ct = default);
    Task<List<WalletTransactionApiDto>> GetWalletTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<BankOptionApiDto>> GetAvailableBanksAsync(CancellationToken ct = default);
    Task<List<BankAccountApiDto>> GetBankAccountsAsync(CancellationToken ct = default);
    Task AddBankAccountAsync(int bankId, string iban, CancellationToken ct = default);
    Task UpdateBankAccountAsync(int bankAccountId, int bankId, string iban, CancellationToken ct = default);
    Task DeleteBankAccountAsync(int bankAccountId, CancellationToken ct = default);
    Task RequestWithdrawalAsync(decimal amount, int bankAccountId, CancellationToken ct = default);

    // Profile
    Task<ProfileApiDto?> GetProfileAsync(CancellationToken ct = default);
    Task<ProfileApiDto?> UpdateProfileAsync(object dto, CancellationToken ct = default);
    Task<string?> UploadProfilePhotoAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);

    // Lookups (authenticated)
    Task<List<LookupItemDto>> GetCitiesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetDistrictsAsync(int cityId, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetProfessionsAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetEducationLevelsAsync(CancellationToken ct = default);

    // Lookups (register — no auth needed)
    Task<List<LookupItemDto>> GetRegisterCitiesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetRegisterDistrictsAsync(int cityId, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetRegisterProfessionsAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetRegisterEducationLevelsAsync(CancellationToken ct = default);
    Task<List<BankOptionApiDto>> GetRegisterBankOptionsAsync(CancellationToken ct = default);

    // App Content
    Task<AppContentApiDto?> GetAppContentAsync(CancellationToken ct = default);

    // FCM
    Task UpdateFcmTokenAsync(string fcmToken, CancellationToken ct = default);

    Task PingAsync(CancellationToken ct = default);

    // Media (auth-required)
    Task<byte[]?> GetImageBytesAsync(string url, CancellationToken ct = default);
}
