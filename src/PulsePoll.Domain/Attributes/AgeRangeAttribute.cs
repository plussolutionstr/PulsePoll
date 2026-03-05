using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class AgeRangeAttribute(int minimumAge = 18, int maximumAge = 90, bool allowExactBoundaries = true)
    : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return new ValidationResult("Doğum tarihi zorunludur.");

        DateOnly? birthDate = value switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => null
        };

        if (birthDate is null)
            return new ValidationResult("Geçersiz tarih formatı.");

        var today = DateOnly.FromDateTime(TurkeyTime.Today);

        if (birthDate > today)
            return new ValidationResult("Doğum tarihi gelecekte olamaz.");

        var minBirthDate = today.AddYears(-minimumAge);
        if (allowExactBoundaries ? birthDate > minBirthDate : birthDate >= minBirthDate)
            return new ValidationResult($"En az {minimumAge} yaşında olmalısınız.");

        var maxBirthDate = today.AddYears(-maximumAge - 1);
        if (allowExactBoundaries ? birthDate < maxBirthDate : birthDate <= maxBirthDate)
            return new ValidationResult($"En fazla {maximumAge} yaşında olabilirsiniz.");

        return ValidationResult.Success;
    }
}
