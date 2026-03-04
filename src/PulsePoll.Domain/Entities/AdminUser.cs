using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class AdminUser : EntityBase
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<AdminUserRole> AdminUserRoles { get; set; } = [];
}
