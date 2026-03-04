namespace PulsePoll.Domain.Entities;

public class Role : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AdminUserRole> AdminUserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
