using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class PaymentSettingRepository(AppDbContext db) : IPaymentSettingRepository
{
    public Task<List<PaymentSetting>> GetAllAsync()
        => db.PaymentSettings.OrderBy(s => s.Key).ToListAsync();

    public Task<PaymentSetting?> GetByKeyAsync(string key)
        => db.PaymentSettings.FirstOrDefaultAsync(s => s.Key == key);

    public async Task UpsertAsync(string key, string value, int adminId, string? description = null)
    {
        var setting = await db.PaymentSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            setting = new PaymentSetting { Key = key, Value = value, Description = description };
            setting.SetCreated(adminId);
            db.PaymentSettings.Add(setting);
        }
        else
        {
            setting.Value       = value;
            if (description is not null) setting.Description = description;
            setting.SetUpdated(adminId);
            db.PaymentSettings.Update(setting);
        }
        await db.SaveChangesAsync();
    }
}
