using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record SubjectDto(
    int Id,
    Guid PublicId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string PhoneNumber,
    Gender Gender,
    int Age,
    DateOnly BirthDate,
    MaritalStatus MaritalStatus,
    GsmOperator GsmOperator,
    string CityName,
    string DistrictName,
    string ProfessionName,
    string EducationLevelName,
    bool IsRetired,
    bool IsHeadOfFamily,
    bool IsHeadOfFamilyRetired,
    string? HeadOfFamilyProfessionName,
    string? HeadOfFamilyEducationLevelName,
    string BankName,
    string IBAN,
    string IBANFullName,
    string SocioeconomicStatusName,
    string? SocioeconomicStatusCode,
    string LSMSocioeconomicStatusName,
    string? LSMSocioeconomicStatusCode,
    string? ReferenceCode,
    string ReferralCode,
    ApprovalStatus Status,
    DateTime CreatedAt,
    decimal? Score,
    int? Star);

public record RegisterSubjectDto(
    string RegistrationToken,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Gender Gender,
    DateOnly BirthDate,
    MaritalStatus MaritalStatus,
    GsmOperator GsmOperator,
    int CityId,
    int DistrictId,
    bool IsRetired,
    int ProfessionId,
    int EducationLevelId,
    bool IsHeadOfFamily,
    bool IsHeadOfFamilyRetired,
    int? HeadOfFamilyProfessionId,
    int? HeadOfFamilyEducationLevelId,
    int BankId,
    string IBAN,
    string IBANFullName,
    string? ReferenceCode,
    bool KVKKApproval,
    string? KVKKDetail);

public record SendOtpDto(string PhoneNumber);

public record VerifyOtpDto(string PhoneNumber, string Otp);

public record OtpVerifiedDto(string RegistrationToken);

public record LoginDto(string Email, string Password);

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
