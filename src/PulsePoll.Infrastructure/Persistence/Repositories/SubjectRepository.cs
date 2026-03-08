using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectRepository(AppDbContext db) : ISubjectRepository
{
    public Task<Subject?> GetByIdAsync(int id)
        => db.Subjects
             .Include(s => s.City)
             .Include(s => s.District)
             .Include(s => s.Profession)
             .Include(s => s.EducationLevel)
             .Include(s => s.BankAccounts)
             .Include(s => s.SocioeconomicStatus)
             .Include(s => s.LSMSocioeconomicStatus)
             .Include(s => s.HeadOfFamilyProfession)
             .Include(s => s.HeadOfFamilyEducationLevel)
             .FirstOrDefaultAsync(s => s.Id == id);

    public Task<Subject?> GetByEmailAsync(string email)
        => db.Subjects.FirstOrDefaultAsync(s => s.Email == email);

    public Task<Subject?> GetByPhoneAsync(string phoneNumber)
        => db.Subjects.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

    public Task<Subject?> GetByPublicIdAsync(Guid publicId)
        => db.Subjects.FirstOrDefaultAsync(s => s.PublicId == publicId);

    public Task<Subject?> GetByReferralCodeAsync(string referralCode)
        => db.Subjects.FirstOrDefaultAsync(s => s.ReferralCode == referralCode);

    public Task<bool> ExistsByEmailAsync(string email)
        => db.Subjects.AnyAsync(s => s.Email == email);

    public Task<bool> ExistsByPhoneAsync(string phoneNumber)
        => db.Subjects.AnyAsync(s => s.PhoneNumber == phoneNumber);

    public async Task AddAsync(Subject subject)
    {
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subject subject)
    {
        db.Subjects.Update(subject);
        await db.SaveChangesAsync();
    }

    public Task<List<Subject>> GetAllAsync()
        => db.Subjects
             .Include(s => s.City)
             .Include(s => s.District)
             .Include(s => s.Profession)
             .Include(s => s.EducationLevel)
             .Include(s => s.BankAccounts)
             .Include(s => s.SocioeconomicStatus)
             .Include(s => s.LSMSocioeconomicStatus)
             .Include(s => s.HeadOfFamilyProfession)
             .Include(s => s.HeadOfFamilyEducationLevel)
             .OrderByDescending(s => s.CreatedAt)
             .ToListAsync();

    public Task<List<Subject>> GetByIdsAsync(List<int> ids)
        => db.Subjects
             .Where(s => ids.Contains(s.Id))
             .ToListAsync();

    public Task<List<int>> GetAllIdsAsync()
        => db.Subjects
             .AsNoTracking()
             .Select(s => s.Id)
             .ToListAsync();

    public Task<Dictionary<int, string>> GetFcmTokensByIdsAsync(IEnumerable<int> subjectIds)
        => db.Subjects
             .Where(s => subjectIds.Contains(s.Id) && s.FcmToken != null)
             .Select(s => new { s.Id, Token = s.FcmToken! })
             .ToDictionaryAsync(s => s.Id, s => s.Token);

    public Task<List<Referral>> GetReferralsAsync(int referrerId)
        => db.Referrals
             .AsNoTracking()
             .Where(r => r.ReferrerId == referrerId)
             .Include(r => r.ReferredSubject)
             .OrderByDescending(r => r.ReferredAt)
             .ToListAsync();

    public Task<Referral?> GetReferralByReferredSubjectIdAsync(int referredSubjectId)
        => db.Referrals
             .Include(r => r.ReferredSubject)
             .FirstOrDefaultAsync(r => r.ReferredSubjectId == referredSubjectId);

    public Task<List<Referral>> GetPendingRewardReferralsAsync()
        => db.Referrals
             .AsNoTracking()
             .Include(r => r.ReferredSubject)
             .Where(r => r.CommissionEarned == null && r.DeletedAt == null)
             .OrderBy(r => r.Id)
             .ToListAsync();

    public async Task AddReferralAsync(Referral referral)
    {
        db.Referrals.Add(referral);
        await db.SaveChangesAsync();
    }

    public async Task UpdateReferralAsync(Referral referral)
    {
        db.Referrals.Update(referral);
        await db.SaveChangesAsync();
    }

    public Task<int> GetReferralCountAsync(int referrerId)
        => db.Referrals.CountAsync(r => r.ReferrerId == referrerId);

    public Task<decimal> GetReferralCommissionAsync(int referrerId)
        => db.Referrals
             .Where(r => r.ReferrerId == referrerId && r.CommissionEarned.HasValue)
             .SumAsync(r => r.CommissionEarned ?? 0);

    public Task<decimal> GetReferralCommissionTryAsync(int referrerId)
        => db.Referrals
             .Where(r => r.ReferrerId == referrerId && r.CommissionAmountTry.HasValue)
             .SumAsync(r => r.CommissionAmountTry ?? 0);
}
