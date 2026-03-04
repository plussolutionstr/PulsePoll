using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RequiredIfAttribute(string dependentProperty, object targetValue) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var property = instance.GetType().GetProperty(dependentProperty);

        if (property is null)
            return new ValidationResult($"Bilinmeyen alan: {dependentProperty}");

        var actualValue = property.GetValue(instance);

        if (!Equals(actualValue, targetValue))
            return ValidationResult.Success;

        if (value is null || (value is string s && string.IsNullOrWhiteSpace(s)))
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} zorunludur.");

        return ValidationResult.Success;
    }
}
