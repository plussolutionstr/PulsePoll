namespace PulsePoll.Application.DTOs;

public record CustomerDto(
    int Id,
    string Code,
    string Title,
    string ShortName,
    string TaxNumber,
    int TaxOfficeId,
    string TaxOfficeName,
    string Phone1,
    string? Phone2,
    string? Mobile,
    string Email,
    int CityId,
    string CityName,
    int DistrictId,
    string DistrictName,
    string Address,
    string? LogoUrl,
    DateTime CreatedAt);

public record CreateCustomerDto(
    string Code,
    string Title,
    string ShortName,
    string TaxNumber,
    int TaxOfficeId,
    string Phone1,
    string? Phone2,
    string? Mobile,
    string Email,
    int CityId,
    int DistrictId,
    string Address,
    Stream? LogoStream = null,
    string? LogoFileName = null);

public record UpdateCustomerDto(
    string Title,
    string ShortName,
    string TaxNumber,
    int TaxOfficeId,
    string Phone1,
    string? Phone2,
    string? Mobile,
    string Email,
    int CityId,
    int DistrictId,
    string Address,
    Stream? LogoStream = null,
    string? LogoFileName = null);
