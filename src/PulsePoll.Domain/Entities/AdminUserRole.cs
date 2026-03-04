namespace PulsePoll.Domain.Entities;

public class AdminUserRole
{
    public int AdminUserId { get; set; }
    public int RoleId { get; set; }

    public AdminUser AdminUser { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
