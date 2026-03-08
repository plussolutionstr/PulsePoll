using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.Constants;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Seeding;

public static class PermissionSeeder
{
    // All permission codes: "Module.Action"
    private static readonly (string Code, string Name, string Module)[] AllPermissions =
    [
        (PermissionCodes.Dashboard.View, "Dashboard Görüntüle", "Dashboard"),

        (PermissionCodes.Customers.View, "Müşterileri Görüntüle", "Customers"),
        (PermissionCodes.Customers.Create, "Müşteri Oluştur", "Customers"),
        (PermissionCodes.Customers.Edit, "Müşteri Düzenle", "Customers"),
        (PermissionCodes.Customers.Delete, "Müşteri Sil", "Customers"),

        (PermissionCodes.Projects.View, "Projeleri Görüntüle", "Projects"),
        (PermissionCodes.Projects.Create, "Proje Oluştur", "Projects"),
        (PermissionCodes.Projects.Edit, "Proje Düzenle", "Projects"),
        (PermissionCodes.Projects.Delete, "Proje Sil", "Projects"),
        (PermissionCodes.Projects.ManageSubjects, "Proje Deneklerini Yönet", "Projects"),
        (PermissionCodes.Projects.ManageRewards, "Proje Ödüllerini Yönet", "Projects"),
        (PermissionCodes.Projects.ToggleStatus, "Proje Durumunu Değiştir", "Projects"),
        (PermissionCodes.Projects.StatementView, "Proje Ekstresini Görüntüle", "Projects"),

        (PermissionCodes.Subjects.View, "Denekleri Görüntüle", "Subjects"),
        (PermissionCodes.Subjects.Create, "Denek Oluştur", "Subjects"),
        (PermissionCodes.Subjects.Edit, "Denek Düzenle", "Subjects"),
        (PermissionCodes.Subjects.Approve, "Denek Onayla", "Subjects"),
        (PermissionCodes.Subjects.Reject, "Denek Reddet", "Subjects"),
        (PermissionCodes.Subjects.SendSms, "Denek SMS Gönder", "Subjects"),
        (PermissionCodes.Subjects.SendPush, "Denek Push Gönder", "Subjects"),
        (PermissionCodes.Subjects.LedgerView, "Denek Ekstresini Görüntüle", "Subjects"),
        (PermissionCodes.Subjects.LedgerManage, "Denek Ekstresini Yönet", "Subjects"),
        (PermissionCodes.Subjects.ProjectsView, "Denek Projelerini Görüntüle", "Subjects"),
        (PermissionCodes.Subjects.ReferralsView, "Denek Referanslarını Görüntüle", "Subjects"),
        (PermissionCodes.Subjects.ScoringSettings, "Denek Skor Ayarları", "Subjects"),
        (PermissionCodes.Subjects.RewardUnitSettings, "Ödül Birimi Ayarları", "Subjects"),

        (PermissionCodes.MediaLibrary.View, "Medya Kütüphanesini Görüntüle", "MediaLibrary"),
        (PermissionCodes.MediaLibrary.Upload, "Medya Yükle", "MediaLibrary"),
        (PermissionCodes.MediaLibrary.Delete, "Medya Sil", "MediaLibrary"),

        (PermissionCodes.Stories.View, "Story Yönetimini Görüntüle", "Stories"),
        (PermissionCodes.Stories.Create, "Story Oluştur", "Stories"),
        (PermissionCodes.Stories.Edit, "Story Düzenle", "Stories"),
        (PermissionCodes.Stories.Delete, "Story Sil", "Stories"),
        (PermissionCodes.Stories.Reorder, "Story Sıralama", "Stories"),

        (PermissionCodes.News.View, "Haber Yönetimini Görüntüle", "News"),
        (PermissionCodes.News.Create, "Haber Oluştur", "News"),
        (PermissionCodes.News.Edit, "Haber Düzenle", "News"),
        (PermissionCodes.News.Delete, "Haber Sil", "News"),
        (PermissionCodes.News.Reorder, "Haber Sıralama", "News"),

        (PermissionCodes.Payments.WithdrawalsView, "Çekim Taleplerini Görüntüle", "Payments"),
        (PermissionCodes.Payments.WithdrawalsApprove, "Çekim Talebini Onayla", "Payments"),
        (PermissionCodes.Payments.WithdrawalsReject, "Çekim Talebini Reddet", "Payments"),
        (PermissionCodes.Payments.WithdrawalsExport, "Çekim Taleplerini Dışa Aktar", "Payments"),
        (PermissionCodes.Payments.BatchesView, "Ödeme Paketlerini Görüntüle", "Payments"),
        (PermissionCodes.Payments.BatchesCreate, "Ödeme Paketi Oluştur", "Payments"),
        (PermissionCodes.Payments.BatchesManage, "Ödeme Paketini Yönet", "Payments"),
        (PermissionCodes.Payments.BatchesExport, "Ödeme Paketlerini Dışa Aktar", "Payments"),
        (PermissionCodes.Payments.SettingsView, "Ödeme Ayarlarını Görüntüle", "Payments"),
        (PermissionCodes.Payments.SettingsEdit, "Ödeme Ayarlarını Düzenle", "Payments"),

        (PermissionCodes.Notifications.View, "Bildirim Geçmişini Görüntüle", "Communications"),

        (PermissionCodes.Communications.View, "İletişim Modülünü Görüntüle", "Communications"),
        (PermissionCodes.Communications.Edit, "İletişim Modülünü Düzenle", "Communications"),
        (PermissionCodes.Communications.Sync, "İletişim Senkronizasyonu Çalıştır", "Communications"),
        (PermissionCodes.Communications.Run, "İletişim Kampanyası Çalıştır", "Communications"),

        (PermissionCodes.Settings.View, "Ayarlar Sayfasını Görüntüle", "Settings"),
        (PermissionCodes.Settings.ReferralRewardEdit, "Referans Ödül Ayarlarını Düzenle", "Settings"),
        (PermissionCodes.Settings.AppContentEdit, "Mobil İçerik Ayarlarını Düzenle", "Settings"),
        (PermissionCodes.Settings.NotificationDistributionEdit, "Bildirim Dağıtım Ayarlarını Düzenle", "Settings"),

        (PermissionCodes.AdminUsers.View, "Kullanıcıları Görüntüle", "AdminUsers"),
        (PermissionCodes.AdminUsers.Create, "Kullanıcı Oluştur", "AdminUsers"),
        (PermissionCodes.AdminUsers.Edit, "Kullanıcı Düzenle", "AdminUsers"),
        (PermissionCodes.AdminUsers.Activate, "Kullanıcı Aktif/Pasif", "AdminUsers"),

        (PermissionCodes.Roles.View, "Rolleri Görüntüle", "Roles"),
        (PermissionCodes.Roles.Create, "Rol Oluştur", "Roles"),
        (PermissionCodes.Roles.Edit, "Rol Düzenle", "Roles"),
        (PermissionCodes.Roles.Delete, "Rol Sil", "Roles"),
        (PermissionCodes.Roles.ManagePermissions, "Rol Yetkilerini Yönet", "Roles"),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var expectedPermissions = AllPermissions.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        // Upsert permissions
        var existingCodes = await db.Permissions.Select(p => p.Code).ToHashSetAsync();

        var toAdd = AllPermissions
            .Where(p => !existingCodes.Contains(p.Code))
            .Select(p =>
            {
                var perm = new Permission { Code = p.Code, Name = p.Name, Module = p.Module, IsActive = true };
                perm.SetCreated(0);
                return perm;
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            db.Permissions.AddRange(toAdd);
            await db.SaveChangesAsync();
            log.LogInformation("Seeded {Count} new permissions", toAdd.Count);
        }

        // Align existing permission names/modules/status with constants.
        var existingPermissions = await db.Permissions.ToListAsync();
        var updatedCount = 0;
        var deactivatedLegacyCount = 0;
        foreach (var permission in existingPermissions)
        {
            if (!expectedPermissions.TryGetValue(permission.Code, out var expected))
            {
                if (permission.IsActive)
                {
                    permission.IsActive = false;
                    permission.SetUpdated(0);
                    deactivatedLegacyCount++;
                }
                continue;
            }

            var nameChanged = !string.Equals(permission.Name, expected.Name, StringComparison.Ordinal);
            var moduleChanged = !string.Equals(permission.Module, expected.Module, StringComparison.Ordinal);
            var activeChanged = !permission.IsActive;
            var deletedChanged = permission.DeletedAt is not null;
            if (!nameChanged && !moduleChanged && !activeChanged && !deletedChanged)
                continue;

            permission.Name = expected.Name;
            permission.Module = expected.Module;
            permission.IsActive = true;
            if (permission.DeletedAt is not null)
                permission.Restore(0);
            permission.SetUpdated(0);
            updatedCount++;
        }

        if (updatedCount > 0 || deactivatedLegacyCount > 0)
        {
            await db.SaveChangesAsync();
            log.LogInformation(
                "Aligned {AlignedCount} existing permissions and deactivated {LegacyCount} legacy permissions",
                updatedCount,
                deactivatedLegacyCount);
        }

        // Ensure SuperAdmin role exists with all permissions
        var superAdmin = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        if (superAdmin is null)
        {
            superAdmin = new Role { Name = "SuperAdmin", Description = "Tam yetkili admin", IsActive = true };
            superAdmin.SetCreated(0);
            db.Roles.Add(superAdmin);
            await db.SaveChangesAsync();
            log.LogInformation("Created SuperAdmin role");
        }

        // SuperAdmin must stay exactly in sync with known active permission set.
        var expectedCodes = expectedPermissions.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var assignablePermissionIds = await db.Permissions
            .Where(p => p.DeletedAt == null && p.IsActive && expectedCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync();

        var targetPermissionIdSet = assignablePermissionIds.ToHashSet();
        var currentPermissionIdSet = superAdmin.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        var extraPermissions = superAdmin.RolePermissions
            .Where(rp => !targetPermissionIdSet.Contains(rp.PermissionId))
            .ToList();
        if (extraPermissions.Count > 0)
            db.RolePermissions.RemoveRange(extraPermissions);

        var missingPermissions = targetPermissionIdSet
            .Where(id => !currentPermissionIdSet.Contains(id))
            .ToList();
        if (missingPermissions.Count > 0)
        {
            db.RolePermissions.AddRange(missingPermissions.Select(permissionId => new RolePermission
            {
                RoleId = superAdmin.Id,
                PermissionId = permissionId
            }));
        }

        if (extraPermissions.Count > 0 || missingPermissions.Count > 0)
        {
            await db.SaveChangesAsync();
            log.LogInformation(
                "Synchronized SuperAdmin permissions. Added={AddedCount} Removed={RemovedCount}",
                missingPermissions.Count,
                extraPermissions.Count);
        }

        // Create initial admin user if none exists
        var hasAdmin = await db.AdminUsers.AnyAsync();
        if (!hasAdmin)
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var adminUser = new AdminUser
            {
                FirstName    = "Super",
                LastName     = "Admin",
                Email        = "admin@pulsepoll.com",
                PasswordHash = hasher.Hash("Admin123!"),
                IsActive     = true,
            };
            adminUser.SetCreated(0);
            db.AdminUsers.Add(adminUser);
            await db.SaveChangesAsync();

            db.AdminUserRoles.Add(new AdminUserRole
            {
                AdminUserId = adminUser.Id,
                RoleId      = superAdmin.Id
            });
            await db.SaveChangesAsync();
            log.LogInformation("Created initial admin user: admin@pulsepoll.com");
        }
    }
}
