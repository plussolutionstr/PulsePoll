using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject?> GetByEmailAsync(string email);
    Task<Subject?> GetByPhoneAsync(string phoneNumber);
    Task<Subject?> GetByReferralCodeAsync(string referralCode);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByPhoneAsync(string phoneNumber);
    Task AddAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task<List<Subject>> GetAllAsync();
    Task<List<int>> GetAllIdsAsync();
    Task<Dictionary<int, string>> GetFcmTokensByIdsAsync(IEnumerable<int> subjectIds);
    Task<List<Referral>> GetReferralsAsync(int referrerId);
    Task<Referral?> GetReferralByReferredSubjectIdAsync(int referredSubjectId);
    Task AddReferralAsync(Referral referral);
    Task UpdateReferralAsync(Referral referral);
    Task<int> GetReferralCountAsync(int referrerId);
    Task<decimal> GetReferralCommissionAsync(int referrerId);
    Task<decimal> GetReferralCommissionTryAsync(int referrerId);
}
