using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PulsePoll.Domain.Attributes;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class Subject : EntityBase
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public Gender Gender { get; set; }

    [Required]
    [AgeRange(minimumAge: 18, maximumAge: 90)]
    public DateOnly BirthDate { get; set; }

    [NotMapped]
    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age)) age--;
            return age;
        }
    }

    public MaritalStatus MaritalStatus { get; set; }

    public GsmOperator GsmOperator { get; set; }

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int CityId { get; set; }

    public int DistrictId { get; set; }

    public bool IsRetired { get; set; }

    public int ProfessionId { get; set; }

    public int EducationLevelId { get; set; }

    public bool IsHeadOfFamily { get; set; }

    public bool IsHeadOfFamilyRetired { get; set; }

    [RequiredIf(nameof(IsHeadOfFamily), false)]
    public int? HeadOfFamilyProfessionId { get; set; }

    [RequiredIf(nameof(IsHeadOfFamily), false)]
    public int? HeadOfFamilyEducationLevelId { get; set; }

    public int SocioeconomicStatusId { get; set; }

    public int LSMSocioeconomicStatusId { get; set; }

    [MaxLength(50)]
    public string? ReferenceCode { get; set; }

    [Required, MaxLength(10)]
    public string ReferralCode { get; set; } = string.Empty;

    public int? SpecialCodeId { get; set; }

    public bool KVKKApproval { get; set; }

    [MaxLength(2000)]
    public string? KVKKDetail { get; set; }

    public DateTime? KVKKApprovalDate { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    [MaxLength(512)]
    public string? FcmToken { get; set; }

    // Navigation properties
    public City City { get; set; } = null!;
    public District District { get; set; } = null!;
    public Profession Profession { get; set; } = null!;
    public EducationLevel EducationLevel { get; set; } = null!;
    public Profession? HeadOfFamilyProfession { get; set; }
    public EducationLevel? HeadOfFamilyEducationLevel { get; set; }
    public SocioeconomicStatus SocioeconomicStatus { get; set; } = null!;
    public LSMSocioeconomicStatus LSMSocioeconomicStatus { get; set; } = null!;
    public SpecialCode? SpecialCode { get; set; }
    public ICollection<BankAccount> BankAccounts { get; set; } = [];

    public SubjectScoreSnapshot? ScoreSnapshot { get; set; }

    // Referral navigation properties
    public ICollection<Referral> ReferralsGiven { get; set; } = [];
    public ICollection<Referral> ReferredBy { get; set; } = [];
}
