using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface IPulsePollApiClient
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default);
    Task MarkStorySeenAsync(int storyId, CancellationToken ct = default);
    Task<List<NewsModel>> GetNewsAsync(CancellationToken ct = default);
    Task<List<SurveyModel>> GetProjectsAsync(CancellationToken ct = default);
    Task<SurveyModel?> GetProjectByIdAsync(int projectId, CancellationToken ct = default);
    Task<List<HistoryItemModel>> GetHistoryAsync(CancellationToken ct = default);
    Task<WalletApiDto?> GetWalletAsync(CancellationToken ct = default);
    Task<List<WalletTransactionApiDto>> GetWalletTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<BankOptionApiDto>> GetAvailableBanksAsync(CancellationToken ct = default);
    Task<List<BankAccountApiDto>> GetBankAccountsAsync(CancellationToken ct = default);
    Task AddBankAccountAsync(int bankId, string iban, CancellationToken ct = default);
    Task UpdateBankAccountAsync(int bankAccountId, int bankId, string iban, CancellationToken ct = default);
    Task DeleteBankAccountAsync(int bankAccountId, CancellationToken ct = default);
    Task RequestWithdrawalAsync(decimal amount, int bankAccountId, CancellationToken ct = default);
    Task<string> StartProjectAsync(int projectId, CancellationToken ct = default);
    Task SubmitProjectResultAsync(int projectId, string status, string? rawPayload = null, CancellationToken ct = default);
    Task<ProfileApiDto?> GetProfileAsync(CancellationToken ct = default);
    Task<ProfileApiDto?> UpdateProfileAsync(object dto, CancellationToken ct = default);
    Task<string?> UploadProfilePhotoAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetCitiesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetDistrictsAsync(int cityId, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetProfessionsAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetEducationLevelsAsync(CancellationToken ct = default);
    Task PingAsync(CancellationToken ct = default);
}
